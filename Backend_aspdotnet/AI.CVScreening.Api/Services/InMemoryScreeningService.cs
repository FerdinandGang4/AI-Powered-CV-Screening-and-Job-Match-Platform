using AI.CVScreening.Api.Models.Candidates;
using AI.CVScreening.Api.Models.Documents;
using AI.CVScreening.Api.Models.Evaluations;
using AI.CVScreening.Api.Models.JobPostings;
using AI.CVScreening.Api.Models.Shared;
using AI.CVScreening.Api.Models.Uploads;
using System.Text.RegularExpressions;

namespace AI.CVScreening.Api.Services;

public sealed class InMemoryScreeningService(
    IJobPostingService jobPostingService,
    ICandidateService candidateService,
    IOpenAiRankingService openAiRankingService,
    ILogger<InMemoryScreeningService> logger,
    AppMemoryStore store) : IScreeningService
{
    public async Task<ScreeningBatchUploadResponse> CreateBatchAsync(
        ScreeningBatchUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var jobPostingId = request.JobPostingId
            ?? jobPostingService.GetAll().FirstOrDefault()?.Id
            ?? throw new InvalidOperationException("No job posting is available for candidate screening.");

        var jobPosting = jobPostingService.GetById(jobPostingId)
            ?? throw new InvalidOperationException("Unable to find the selected job posting.");

        var jobDescriptionText = ReadUploadedText(request.JobDescriptionFile);
        var uploadedCandidates = request.CandidateCvs
            .Select(candidate => BuildCandidateFromUpload(candidate, jobPosting, jobDescriptionText))
            .ToArray();

        var rankingReport = await BuildRankingReportAsync(jobPosting, uploadedCandidates, jobDescriptionText, cancellationToken);

        var batchId = Guid.NewGuid();
        store.ReportsByBatchId[batchId] = rankingReport;
        store.BatchToJobPostingMap[batchId] = jobPostingId;
        store.LatestReportsByJobPostingId[jobPostingId] = rankingReport;

        return new ScreeningBatchUploadResponse
        {
            BatchId = batchId,
            JobPostingId = jobPostingId,
            JobDescription = new UploadedDocumentResultDto
            {
                DocumentId = Guid.NewGuid(),
                FileName = request.JobDescriptionFile?.FileName ?? "job-description.txt",
                ContentType = request.JobDescriptionFile?.ContentType ?? "text/plain",
                ProcessingStatus = DocumentProcessingStatus.Uploaded
            },
            CandidateDocuments = request.CandidateCvs
                .Select(candidate => new UploadedDocumentResultDto
                {
                    DocumentId = Guid.NewGuid(),
                    FileName = candidate.CvFile?.FileName ?? $"{candidate.CandidateName}-cv.pdf",
                    ContentType = candidate.CvFile?.ContentType ?? "application/pdf",
                    ProcessingStatus = DocumentProcessingStatus.Uploaded
                })
                .ToArray(),
            Message = "Documents uploaded successfully. A ranking report has been generated for the selected job posting."
        };
    }

    public RankingReportDto? GetRankingReportByBatchId(Guid batchId)
    {
        return store.ReportsByBatchId.GetValueOrDefault(batchId);
    }

    public RankingReportDto? GetRankingReport(Guid jobPostingId)
    {
        if (store.LatestReportsByJobPostingId.TryGetValue(jobPostingId, out var latestReport))
        {
            return latestReport;
        }

        var jobPosting = jobPostingService.GetById(jobPostingId);
        if (jobPosting is null)
        {
            return null;
        }

        var fallbackCandidates = candidateService.GetAll()
            .Select(candidate => new CandidateInput(
                candidate,
                new RankingCandidateInput(
                    candidate.Id,
                    candidate.FullName,
                    candidate.Email,
                    candidate.TotalYearsExperience,
                    candidate.Skills.Select(skill => skill.SkillName).ToArray(),
                    Array.Empty<string>(),
                    string.Empty)))
            .ToArray();

        return BuildHeuristicRankingReport(jobPosting, fallbackCandidates, jobPosting.DescriptionText);
    }

    private async Task<RankingReportDto> BuildRankingReportAsync(
        JobPostingDetailDto jobPosting,
        IReadOnlyCollection<CandidateInput> candidates,
        string jobDescriptionText,
        CancellationToken cancellationToken)
    {
        if (openAiRankingService.IsConfigured)
        {
            try
            {
                var aiRankedCandidates = await openAiRankingService.RankCandidatesAsync(
                    jobPosting,
                    jobDescriptionText,
                    candidates.Select(candidate => candidate.RankingInput).ToArray(),
                    cancellationToken);

                return BuildAiRankingReport(jobPosting, candidates, aiRankedCandidates);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "AI ranking failed. Falling back to local heuristic scoring.");
            }
        }

        return BuildHeuristicRankingReport(jobPosting, candidates, jobDescriptionText);
    }

    private static RankingReportDto BuildHeuristicRankingReport(
        JobPostingDetailDto jobPosting,
        IReadOnlyCollection<CandidateInput> candidates,
        string jobDescriptionText)
    {
        var rankedCandidates = candidates
            .Select(candidate => BuildEvaluation(candidate, jobPosting, jobDescriptionText))
            .OrderByDescending(evaluation => evaluation.OverallScore)
            .ThenByDescending(evaluation => evaluation.SkillScore)
            .ThenByDescending(evaluation => evaluation.ExperienceScore)
            .ToArray();

        return new RankingReportDto
        {
            Id = Guid.NewGuid(),
            JobPosting = MapToSummary(jobPosting),
            GeneratedAtUtc = DateTime.UtcNow,
            AiUsed = false,
            TotalCandidates = rankedCandidates.Length,
            TopCandidateId = rankedCandidates.FirstOrDefault()?.Candidate.Id,
            RankedCandidates = rankedCandidates
        };
    }

    private static RankingReportDto BuildAiRankingReport(
        JobPostingDetailDto jobPosting,
        IReadOnlyCollection<CandidateInput> candidates,
        IReadOnlyCollection<AiRankedCandidate> aiRankedCandidates)
    {
        var candidateMap = candidates.ToDictionary(candidate => candidate.Candidate.Id, candidate => candidate);

        var rankedCandidates = aiRankedCandidates
            .Select(aiCandidate =>
            {
                if (!candidateMap.TryGetValue(aiCandidate.CandidateId, out var candidate))
                {
                    return null;
                }

                var trackedRequirements = BuildTrackedRequirements(jobPosting, jobPosting.DescriptionText);
                var missingSkills = aiCandidate.MissingSkills
                    .Where(skill => !string.IsNullOrWhiteSpace(skill))
                    .ToArray();

                return new CandidateEvaluationDto
                {
                    Id = Guid.NewGuid(),
                    Candidate = candidate.Candidate,
                    JobPosting = MapToSummary(jobPosting),
                    OverallScore = Math.Max(0, Math.Min(100, aiCandidate.OverallScore)),
                    SkillScore = Math.Round(aiCandidate.OverallScore * 0.4m, 2),
                    ExperienceScore = Math.Round(aiCandidate.OverallScore * 0.2m, 2),
                    ProjectScore = Math.Round(aiCandidate.OverallScore * 0.15m, 2),
                    EducationScore = Math.Round(aiCandidate.OverallScore * 0.1m, 2),
                    SemanticScore = Math.Round(aiCandidate.OverallScore * 0.15m, 2),
                    Recommendation = aiCandidate.OverallScore >= 80 ? "Strong Match" : aiCandidate.OverallScore >= 60 ? "Consider" : "Weak Match",
                    EvaluatedAtUtc = DateTime.UtcNow,
                    Explanation = new MatchExplanationDto
                    {
                        Id = Guid.NewGuid(),
                        Summary = aiCandidate.Summary,
                        Strengths = aiCandidate.Strengths,
                        Weaknesses = aiCandidate.Weaknesses,
                        Notes = $"AI-ranked against {trackedRequirements.Count} tracked role requirements."
                    },
                    SkillGaps = missingSkills
                        .Select(skill => new SkillGapDto
                        {
                            Id = Guid.NewGuid(),
                            SkillName = skill,
                            GapReason = $"AI ranking marked '{skill}' as missing or too weak for this role.",
                            Severity = trackedRequirements.Any(requirement =>
                                requirement.IsMandatory &&
                                string.Equals(requirement.Name, skill, StringComparison.OrdinalIgnoreCase))
                                ? GapSeverity.High
                                : GapSeverity.Medium
                        })
                        .ToArray()
                };
            })
            .Where(candidate => candidate is not null)
            .Cast<CandidateEvaluationDto>()
            .OrderByDescending(candidate => candidate.OverallScore)
            .ToArray();

        return new RankingReportDto
        {
            Id = Guid.NewGuid(),
            JobPosting = MapToSummary(jobPosting),
            GeneratedAtUtc = DateTime.UtcNow,
            AiUsed = true,
            TotalCandidates = rankedCandidates.Length,
            TopCandidateId = rankedCandidates.FirstOrDefault()?.Candidate.Id,
            RankedCandidates = rankedCandidates
        };
    }

    private static CandidateEvaluationDto BuildEvaluation(
        CandidateInput candidateInput,
        JobPostingDetailDto jobPosting,
        string jobDescriptionText)
    {
        var candidateSkillNames = candidateInput.Candidate.Skills
            .Select(skill => skill.SkillName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var trackedRequirements = BuildTrackedRequirements(jobPosting, jobDescriptionText);

        var matchedRequirements = trackedRequirements
            .Where(requirement => candidateSkillNames.Contains(requirement.Name))
            .ToArray();

        var missingRequirements = trackedRequirements
            .Where(requirement => !candidateSkillNames.Contains(requirement.Name))
            .ToArray();

        var mandatoryGapPenalty = missingRequirements.Count(requirement => requirement.IsMandatory) * 12;

        var skillScore = matchedRequirements.Sum(requirement => requirement.Weight);
        var experienceScore = CalculateExperienceScore(candidateInput.Candidate.TotalYearsExperience, jobPosting.MinimumYearsExperience);
        var projectScore = CalculateProjectScore(candidateInput.ProjectKeywords, trackedRequirements);
        var educationScore = 10;
        var semanticScore = CalculateSemanticScore(candidateInput.RawText, jobDescriptionText, trackedRequirements);
        var overallScore = Math.Max(0, Math.Min(100, skillScore + experienceScore + projectScore + educationScore + semanticScore - mandatoryGapPenalty));

        return new CandidateEvaluationDto
        {
            Id = Guid.NewGuid(),
            Candidate = candidateInput.Candidate,
            JobPosting = MapToSummary(jobPosting),
            OverallScore = overallScore,
            SkillScore = skillScore,
            ExperienceScore = experienceScore,
            ProjectScore = projectScore,
            EducationScore = educationScore,
            SemanticScore = semanticScore,
            Recommendation = overallScore >= 80 ? "Strong Match" : overallScore >= 60 ? "Consider" : "Weak Match",
            EvaluatedAtUtc = DateTime.UtcNow,
            Explanation = new MatchExplanationDto
            {
                Id = Guid.NewGuid(),
                Summary = $"{candidateInput.Candidate.FullName} matched {matchedRequirements.Length} out of {trackedRequirements.Count} tracked requirements.",
                Strengths = matchedRequirements.Length == 0
                    ? "No direct skill matches were found in the current sample profile."
                    : $"Matched skills: {string.Join(", ", matchedRequirements.Select(requirement => requirement.Name))}.",
                Weaknesses = missingRequirements.Length == 0
                    ? "No major requirement gaps were detected."
                    : $"Missing or weak areas: {string.Join(", ", missingRequirements.Select(requirement => requirement.Name))}.",
                Notes = $"Candidate has {candidateInput.Candidate.TotalYearsExperience} years of experience for a role requiring {jobPosting.MinimumYearsExperience}+ years."
            },
            SkillGaps = missingRequirements
                .Select(requirement => new SkillGapDto
                {
                    Id = Guid.NewGuid(),
                    SkillName = requirement.Name,
                    GapReason = $"Requirement '{requirement.Name}' was not detected in the current candidate skill profile.",
                    Severity = requirement.IsMandatory ? GapSeverity.High : GapSeverity.Medium
                })
                .ToArray()
        };
    }

    private static CandidateInput BuildCandidateFromUpload(
        CandidateCvUploadItem upload,
        JobPostingDetailDto jobPosting,
        string jobDescriptionText)
    {
        var rawText = ReadUploadedText(upload.CvFile);
        var detectedSkills = DetectSkills(rawText, jobPosting, jobDescriptionText);
        var detectedProjects = DetectProjectKeywords(rawText, jobPosting, jobDescriptionText);
        var yearsExperience = DetectYearsOfExperience(rawText, jobPosting.MinimumYearsExperience);

        var profile = new CandidateSummaryDto
        {
            Id = Guid.NewGuid(),
            FullName = upload.CandidateName,
            Email = upload.CandidateEmail,
            TotalYearsExperience = yearsExperience,
            CurrentLocation = "Not specified",
            Skills = detectedSkills
                .Select(skill => new CandidateSkillDto
                {
                    Id = Guid.NewGuid(),
                    SkillName = skill,
                    ProficiencyLevel = "Detected",
                    YearsUsed = yearsExperience > 0 ? yearsExperience : 1,
                    LastUsedYear = DateTime.UtcNow.Year
                })
                .ToArray()
        };

        return new CandidateInput(
            profile,
            new RankingCandidateInput(
                profile.Id,
                profile.FullName,
                profile.Email,
                profile.TotalYearsExperience,
                detectedSkills,
                detectedProjects,
                rawText));
    }

    private static IReadOnlyCollection<JobRequirementDto> BuildTrackedRequirements(
        JobPostingDetailDto jobPosting,
        string jobDescriptionText)
    {
        var requirements = new List<JobRequirementDto>();

        requirements.AddRange(jobPosting.Requirements);

        var jobKeywords = ExtractKeywords($"{jobPosting.Title} {jobPosting.DescriptionText} {jobDescriptionText}")
            .Where(keyword => keyword.Length > 2)
            .Take(8);

        foreach (var keyword in jobKeywords)
        {
            if (requirements.Any(requirement => string.Equals(requirement.Name, keyword, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            requirements.Add(new JobRequirementDto
            {
                Id = Guid.NewGuid(),
                Name = keyword,
                RequirementType = RequirementType.Skill,
                Priority = RequirementPriority.Medium,
                Weight = 5,
                IsMandatory = false
            });
        }

        return requirements;
    }

    private static decimal CalculateExperienceScore(decimal candidateYears, int minimumYears)
    {
        if (minimumYears <= 0)
        {
            return 20;
        }

        if (candidateYears >= minimumYears + 2)
        {
            return 20;
        }

        if (candidateYears >= minimumYears)
        {
            return 17;
        }

        if (candidateYears >= minimumYears - 1)
        {
            return 12;
        }

        return 6;
    }

    private static decimal CalculateProjectScore(
        IReadOnlyCollection<string> projectKeywords,
        IReadOnlyCollection<JobRequirementDto> trackedRequirements)
    {
        if (projectKeywords.Count == 0)
        {
            return 6;
        }

        var matchedProjects = trackedRequirements.Count(requirement =>
            projectKeywords.Any(keyword => string.Equals(keyword, requirement.Name, StringComparison.OrdinalIgnoreCase)));

        return Math.Min(15, 6 + (matchedProjects * 3));
    }

    private static decimal CalculateSemanticScore(
        string rawCvText,
        string jobDescriptionText,
        IReadOnlyCollection<JobRequirementDto> trackedRequirements)
    {
        var cvKeywords = ExtractKeywords(rawCvText);
        var jobKeywords = ExtractKeywords($"{jobDescriptionText} {string.Join(' ', trackedRequirements.Select(requirement => requirement.Name))}");

        if (jobKeywords.Count == 0)
        {
            return 6;
        }

        var overlapCount = cvKeywords.Intersect(jobKeywords, StringComparer.OrdinalIgnoreCase).Count();
        var overlapRatio = (decimal)overlapCount / jobKeywords.Count;

        return Math.Min(15, 5 + Math.Round(overlapRatio * 10, 2));
    }

    private static decimal DetectYearsOfExperience(string rawText, int fallbackYears)
    {
        var matches = Regex.Matches(rawText, @"(?<years>\d+(?:\.\d+)?)\s*\+?\s*(?:years|yrs)", RegexOptions.IgnoreCase);
        if (matches.Count == 0)
        {
            return Math.Max(1, fallbackYears);
        }

        var parsedYears = matches
            .Select(match => decimal.TryParse(match.Groups["years"].Value, out var years) ? years : 0)
            .DefaultIfEmpty(fallbackYears)
            .Max();

        return parsedYears <= 0 ? Math.Max(1, fallbackYears) : parsedYears;
    }

    private static string[] DetectSkills(string rawText, JobPostingDetailDto jobPosting, string jobDescriptionText)
    {
        var vocabulary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var requirement in jobPosting.Requirements)
        {
            vocabulary.Add(requirement.Name);
        }

        foreach (var keyword in new[]
                 {
                     "C#", "ASP.NET Core", "ASP.NET", "SQL", "Azure", "React", "Node.js",
                     "MongoDB", "REST API", "Microservices", "Docker", "Kubernetes",
                     "JavaScript", "TypeScript", "Python", "Machine Learning", "NLP"
                 })
        {
            vocabulary.Add(keyword);
        }

        foreach (var keyword in ExtractKeywords($"{jobPosting.Title} {jobPosting.DescriptionText} {jobDescriptionText}"))
        {
            vocabulary.Add(keyword);
        }

        return vocabulary
            .Where(skill => ContainsTerm(rawText, skill))
            .OrderBy(skill => skill)
            .ToArray();
    }

    private static string[] DetectProjectKeywords(string rawText, JobPostingDetailDto jobPosting, string jobDescriptionText)
    {
        return BuildTrackedRequirements(jobPosting, jobDescriptionText)
            .Select(requirement => requirement.Name)
            .Where(requirement => ContainsTerm(rawText, requirement))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string ReadUploadedText(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return string.Empty;
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static HashSet<string> ExtractKeywords(string text)
    {
        return Regex.Matches(text ?? string.Empty, @"[A-Za-z][A-Za-z0-9\.\#\+\-]{2,}")
            .Select(match => match.Value.Trim())
            .Where(value => !StopWords.Contains(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool ContainsTerm(string text, string term)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term))
        {
            return false;
        }

        var matches = Regex.Matches(text, $@"(?<!\w){Regex.Escape(term)}(?!\w)", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            var prefixStart = Math.Max(0, match.Index - 20);
            var prefix = text[prefixStart..match.Index];

            if (Regex.IsMatch(prefix, @"\b(no|not|without|lack(?:ing)?)\s+$", RegexOptions.IgnoreCase))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private sealed record CandidateInput(
        CandidateSummaryDto Candidate,
        RankingCandidateInput RankingInput)
    {
        public IReadOnlyCollection<string> DetectedSkills => RankingInput.Skills;
        public IReadOnlyCollection<string> ProjectKeywords => RankingInput.ProjectKeywords;
        public string RawText => RankingInput.RawText;
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "and", "the", "with", "for", "this", "that", "from", "your", "have", "has",
        "will", "into", "our", "role", "job", "you", "are", "using", "build",
        "candidate", "candidates", "analysis", "ranking", "platform", "system"
    };

    private static JobPostingSummaryDto MapToSummary(JobPostingDetailDto detail)
    {
        return new JobPostingSummaryDto
        {
            Id = detail.Id,
            Title = detail.Title,
            Department = detail.Department,
            DescriptionText = detail.DescriptionText,
            MinimumYearsExperience = detail.MinimumYearsExperience,
            Location = detail.Location,
            CreatedAtUtc = detail.CreatedAtUtc
        };
    }
}
