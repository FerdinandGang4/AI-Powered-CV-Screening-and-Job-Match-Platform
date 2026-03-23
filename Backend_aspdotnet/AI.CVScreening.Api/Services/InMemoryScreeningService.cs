using AI.CVScreening.Api.Models.Candidates;
using AI.CVScreening.Api.Models.Documents;
using AI.CVScreening.Api.Models.Evaluations;
using AI.CVScreening.Api.Models.JobPostings;
using AI.CVScreening.Api.Models.Shared;
using AI.CVScreening.Api.Models.Uploads;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

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
        var jobDescriptionText = ResolveJobDescriptionText(request);
        if (string.IsNullOrWhiteSpace(jobDescriptionText))
        {
            throw new InvalidOperationException("Provide a job description by file upload or pasted text before screening candidates.");
        }

        var jobPosting = ResolveJobPosting(request.JobPostingId, jobDescriptionText);
        var uploadedCandidates = request.CandidateCvs
            .Select(candidate => BuildCandidateFromUpload(candidate, jobPosting, jobDescriptionText))
            .ToArray();

        var rankingReport = await BuildRankingReportAsync(jobPosting, uploadedCandidates, jobDescriptionText, cancellationToken);

        var batchId = Guid.NewGuid();
        store.ReportsByBatchId[batchId] = rankingReport;
        store.BatchToJobPostingMap[batchId] = jobPosting.Id;
        store.LatestReportsByJobPostingId[jobPosting.Id] = rankingReport;

        return new ScreeningBatchUploadResponse
        {
            BatchId = batchId,
            JobPostingId = jobPosting.Id,
            JobDescription = new UploadedDocumentResultDto
            {
                DocumentId = Guid.NewGuid(),
                FileName = request.JobDescriptionFile?.FileName ?? "pasted-job-description.txt",
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

    private static string ResolveJobDescriptionText(ScreeningBatchUploadRequest request)
    {
        var uploadedText = ReadUploadedText(request.JobDescriptionFile);
        if (!string.IsNullOrWhiteSpace(uploadedText))
        {
            return uploadedText;
        }

        return NormalizeExtractedText(request.JobDescriptionText ?? string.Empty);
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
                    string.Empty),
                null))
            .ToArray();

        return BuildHeuristicRankingReport(jobPosting, fallbackCandidates, jobPosting.DescriptionText);
    }

    private JobPostingDetailDto ResolveJobPosting(Guid? requestedJobPostingId, string jobDescriptionText)
    {
        if (requestedJobPostingId.HasValue)
        {
            var selectedJobPosting = jobPostingService.GetById(requestedJobPostingId.Value);
            if (selectedJobPosting is not null)
            {
                return selectedJobPosting;
            }

            logger.LogWarning(
                "Requested job posting {JobPostingId} was not found. Falling back to the latest available job posting.",
                requestedJobPostingId.Value);
        }

        if (!string.IsNullOrWhiteSpace(jobDescriptionText))
        {
            return BuildJobPostingFromInput(jobDescriptionText);
        }

        return jobPostingService.GetAll()
            .Select(summary => jobPostingService.GetById(summary.Id))
            .FirstOrDefault(jobPosting => jobPosting is not null)
            ?? throw new InvalidOperationException("Provide a job description or select an existing job posting before screening candidates.");
    }

    private static JobPostingDetailDto BuildJobPostingFromInput(string jobDescriptionText)
    {
        var detectedSkillRequirements = KnownTechnicalTerms
            .Where(term => ContainsTerm(jobDescriptionText, term))
            .Take(6)
            .Select((term, index) => new JobRequirementDto
            {
                Id = Guid.NewGuid(),
                Name = term,
                RequirementType = RequirementType.Skill,
                Priority = index < 4 ? RequirementPriority.High : RequirementPriority.Medium,
                Weight = index < 4 ? 18 - (index * 2) : 10,
                IsMandatory = index < 4
            })
            .ToList();

        if (ContainsTerm(jobDescriptionText, "Agile"))
        {
            detectedSkillRequirements.Add(new JobRequirementDto
            {
                Id = Guid.NewGuid(),
                Name = "Agile",
                RequirementType = RequirementType.Skill,
                Priority = RequirementPriority.Medium,
                Weight = 8,
                IsMandatory = false
            });
        }

        if (ContainsTerm(jobDescriptionText, "CI/CD"))
        {
            detectedSkillRequirements.Add(new JobRequirementDto
            {
                Id = Guid.NewGuid(),
                Name = "CI/CD",
                RequirementType = RequirementType.Project,
                Priority = RequirementPriority.Medium,
                Weight = 8,
                IsMandatory = false
            });
        }

        return new JobPostingDetailDto
        {
            Id = Guid.NewGuid(),
            Title = DetectJobTitle(jobDescriptionText),
            Department = DetectDepartment(jobDescriptionText),
            DescriptionText = jobDescriptionText,
            MinimumYearsExperience = (int)DetectYearsOfExperience(jobDescriptionText, 3),
            Location = DetectLocation(jobDescriptionText),
            CreatedAtUtc = DateTime.UtcNow,
            Requirements = detectedSkillRequirements
        };
    }

    private static string DetectJobTitle(string jobDescriptionText)
    {
        if (ContainsTerm(jobDescriptionText, ".NET Core") || ContainsTerm(jobDescriptionText, "C#"))
        {
            return "Software Engineer (.NET Core / C#)";
        }

        var firstLine = jobDescriptionText
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(firstLine) ? "Uploaded Job Posting" : firstLine[..Math.Min(firstLine.Length, 80)];
    }

    private static string DetectDepartment(string jobDescriptionText)
    {
        return ContainsTerm(jobDescriptionText, "Engineering") ? "Engineering" : "Technology";
    }

    private static string DetectLocation(string jobDescriptionText)
    {
        if (ContainsTerm(jobDescriptionText, "Remote"))
        {
            return "Remote";
        }

        if (ContainsTerm(jobDescriptionText, "Hybrid"))
        {
            return "Hybrid";
        }

        if (ContainsTerm(jobDescriptionText, "Onsite"))
        {
            return "Onsite";
        }

        return "Not specified";
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

                var aiReport = BuildAiRankingReport(jobPosting, candidates, aiRankedCandidates);
                aiReport.AiStatus = "ai";
                aiReport.AiStatusMessage = "Ranked with OpenAI using the uploaded CV text and job description.";
                return aiReport;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "AI ranking failed. Falling back to local heuristic scoring.");
                var fallbackOnError = BuildHeuristicRankingReport(jobPosting, candidates, jobDescriptionText);
                fallbackOnError.AiStatus = "fallback_error";
                fallbackOnError.AiStatusMessage = "OpenAI ranking was unavailable for this run, so the local scoring engine was used instead.";
                return fallbackOnError;
            }
        }

        var fallbackReport = BuildHeuristicRankingReport(jobPosting, candidates, jobDescriptionText);
        fallbackReport.AiStatus = "fallback_unconfigured";
        fallbackReport.AiStatusMessage = "OpenAI ranking is not configured, so the local scoring engine was used.";
        return fallbackReport;
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
            AiStatus = "fallback",
            AiStatusMessage = "Local scoring was used.",
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
                    ExtractionWarning = candidate.ExtractionWarning,
                    EvaluatedAtUtc = DateTime.UtcNow,
                    Explanation = new MatchExplanationDto
                    {
                        Id = Guid.NewGuid(),
                        Summary = aiCandidate.Summary,
                        Strengths = aiCandidate.Strengths,
                        Weaknesses = aiCandidate.Weaknesses,
                        Notes = string.IsNullOrWhiteSpace(candidate.ExtractionWarning)
                            ? $"AI-ranked against {trackedRequirements.Count} tracked role requirements."
                            : $"AI-ranked against {trackedRequirements.Count} tracked role requirements. Extraction warning: {candidate.ExtractionWarning}"
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
            AiStatus = "ai",
            AiStatusMessage = "Ranked with OpenAI.",
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
        var skillRequirements = trackedRequirements
            .Where(requirement => requirement.RequirementType == RequirementType.Skill)
            .ToArray();
        var projectRequirements = trackedRequirements
            .Where(requirement => requirement.RequirementType == RequirementType.Project)
            .ToArray();
        var totalSkillWeight = skillRequirements.Sum(requirement => requirement.Weight);

        var matchedRequirements = skillRequirements
            .Where(requirement => candidateSkillNames.Contains(requirement.Name))
            .ToArray();

        var missingRequirements = skillRequirements
            .Where(requirement => !candidateSkillNames.Contains(requirement.Name))
            .ToArray();

        var matchedMandatoryRequirements = matchedRequirements.Count(requirement => requirement.IsMandatory);
        var mandatoryRequirementsCount = skillRequirements.Count(requirement => requirement.IsMandatory);
        var mandatoryCoverage = mandatoryRequirementsCount == 0
            ? 1m
            : (decimal)matchedMandatoryRequirements / mandatoryRequirementsCount;
        var matchedSkillWeight = matchedRequirements.Sum(requirement => requirement.Weight);
        var skillCoverage = skillRequirements.Length == 0 ? 0 : (decimal)matchedRequirements.Length / skillRequirements.Length;
        var extractionPenalty = candidateInput.HasWeakExtraction ? 0.78m : 1m;
        var meetsExperienceBaseline = candidateInput.Candidate.TotalYearsExperience >= jobPosting.MinimumYearsExperience;

        var skillScore = totalSkillWeight == 0
            ? 20
            : Math.Round((matchedSkillWeight / totalSkillWeight) * 48m, 2);
        var experienceScore = CalculateExperienceScore(candidateInput.Candidate.TotalYearsExperience, jobPosting.MinimumYearsExperience);
        var projectScore = CalculateProjectScore(candidateInput.ProjectKeywords, projectRequirements);
        var educationScore = candidateInput.HasWeakExtraction ? 2 : 5;
        var semanticScore = CalculateSemanticScore(candidateInput.RawText, jobDescriptionText, skillRequirements);
        var weightedBaseScore = skillScore + experienceScore + projectScore + educationScore + semanticScore;
        var fitMultiplier = 0.68m + (skillCoverage * 0.16m) + (mandatoryCoverage * 0.10m) + (meetsExperienceBaseline ? 0.06m : 0m);
        var overallScore = Math.Round(
            Math.Max(0, Math.Min(100, weightedBaseScore * fitMultiplier * extractionPenalty)),
            2);

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
            ExtractionWarning = candidateInput.ExtractionWarning,
            EvaluatedAtUtc = DateTime.UtcNow,
            Explanation = new MatchExplanationDto
            {
                Id = Guid.NewGuid(),
                Summary = $"{candidateInput.Candidate.FullName} matched {matchedRequirements.Length} of {skillRequirements.Length} core skill requirements and met {matchedMandatoryRequirements} mandatory requirements.",
                Strengths = matchedRequirements.Length == 0
                    ? "No direct skill matches were found in the current sample profile."
                    : $"Matched skills: {string.Join(", ", matchedRequirements.Select(requirement => requirement.Name))}.",
                Weaknesses = missingRequirements.Length == 0
                    ? "No major requirement gaps were detected."
                    : $"Missing or weak areas: {string.Join(", ", missingRequirements.Select(requirement => requirement.Name))}.",
                Notes = BuildReviewNotes(candidateInput, jobPosting)
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
        var yearsExperience = DetectCandidateYearsOfExperience(rawText);
        var extractionWarning = BuildExtractionWarning(upload.CvFile?.FileName, rawText, detectedSkills, yearsExperience);

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
                rawText),
            extractionWarning);
    }

    private static IReadOnlyCollection<JobRequirementDto> BuildTrackedRequirements(
        JobPostingDetailDto jobPosting,
        string jobDescriptionText)
    {
        var requirements = jobPosting.Requirements
            .Select(requirement => new JobRequirementDto
            {
                Id = requirement.Id,
                Name = requirement.Name,
                RequirementType = requirement.RequirementType,
                Priority = requirement.Priority,
                Weight = requirement.Weight,
                IsMandatory = requirement.IsMandatory
            })
            .ToList();

        var detectedTechnicalSkills = KnownTechnicalTerms
            .Where(term => ContainsTerm($"{jobPosting.Title} {jobPosting.DescriptionText} {jobDescriptionText}", term))
            .Where(term => requirements.All(requirement => !string.Equals(requirement.Name, term, StringComparison.OrdinalIgnoreCase)))
            .Take(5);

        foreach (var keyword in detectedTechnicalSkills)
        {
            requirements.Add(new JobRequirementDto
            {
                Id = Guid.NewGuid(),
                Name = keyword,
                RequirementType = RequirementType.Skill,
                Priority = RequirementPriority.Medium,
                Weight = 10,
                IsMandatory = false
            });
        }

        return requirements;
    }

    private static decimal CalculateExperienceScore(decimal candidateYears, int minimumYears)
    {
        if (minimumYears <= 0)
        {
            return 18;
        }

        if (candidateYears >= minimumYears + 2)
        {
            return 20;
        }

        if (candidateYears >= minimumYears)
        {
            return 18;
        }

        if (candidateYears >= minimumYears - 1)
        {
            return 12;
        }

        if (candidateYears >= (minimumYears / 2m))
        {
            return 7;
        }

        return 2;
    }

    private static decimal CalculateProjectScore(
        IReadOnlyCollection<string> projectKeywords,
        IReadOnlyCollection<JobRequirementDto> trackedRequirements)
    {
        if (projectKeywords.Count == 0)
        {
            return 1;
        }

        var matchedProjects = trackedRequirements.Count(requirement =>
            projectKeywords.Any(keyword => string.Equals(keyword, requirement.Name, StringComparison.OrdinalIgnoreCase)));

        if (trackedRequirements.Count == 0)
        {
            return 6;
        }

        return Math.Round(Math.Min(12, 3m + (((decimal)matchedProjects / trackedRequirements.Count) * 9m)), 2);
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
            return 4;
        }

        var overlapCount = cvKeywords.Intersect(jobKeywords, StringComparer.OrdinalIgnoreCase).Count();
        var overlapRatio = (decimal)overlapCount / jobKeywords.Count;

        return Math.Round(Math.Min(10, 2m + (overlapRatio * 8m)), 2);
    }

    private static string BuildReviewNotes(CandidateInput candidateInput, JobPostingDetailDto jobPosting)
    {
        var experienceNote = $"Candidate has {candidateInput.Candidate.TotalYearsExperience} years of experience for a role requiring {jobPosting.MinimumYearsExperience}+ years.";
        if (string.IsNullOrWhiteSpace(candidateInput.ExtractionWarning))
        {
            return experienceNote;
        }

        return $"{experienceNote} Extraction warning: {candidateInput.ExtractionWarning}";
    }

    private static string? BuildExtractionWarning(
        string? fileName,
        string rawText,
        IReadOnlyCollection<string> detectedSkills,
        decimal yearsExperience)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        var textLength = rawText.Trim().Length;

        if (textLength == 0)
        {
            return $"No readable text was extracted from {extension.TrimStart('.').ToUpperInvariant()} CV. This file may be image-based or scanned and may need OCR.";
        }

        if (extension == ".pdf" && textLength < 120)
        {
            return "Very little readable text was extracted from this PDF CV. The match score may be understated unless OCR is added.";
        }

        if (textLength < 220 && detectedSkills.Count == 0 && yearsExperience == 0)
        {
            return "Only limited CV text was extracted, so this ranking is based on incomplete evidence.";
        }

        return null;
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

    private static decimal DetectCandidateYearsOfExperience(string rawText)
    {
        var matches = Regex.Matches(rawText, @"(?<years>\d+(?:\.\d+)?)\s*\+?\s*(?:years|yrs)", RegexOptions.IgnoreCase);
        if (matches.Count == 0)
        {
            return 0;
        }

        var parsedYears = matches
            .Select(match => decimal.TryParse(match.Groups["years"].Value, out var years) ? years : 0)
            .DefaultIfEmpty(0)
            .Max();

        return parsedYears < 0 ? 0 : parsedYears;
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

        foreach (var keyword in KnownTechnicalTerms.Where(term => ContainsTerm($"{jobPosting.Title} {jobPosting.DescriptionText} {jobDescriptionText}", term)))
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

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return extension switch
        {
            ".docx" => ExtractDocxText(file),
            ".pdf" => ExtractPdfText(file),
            _ => ReadStreamAsText(file.OpenReadStream())
        };
    }

    private static string ExtractDocxText(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        memoryStream.Position = 0;

        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, leaveOpen: false);
        var documentEntry = archive.GetEntry("word/document.xml");
        if (documentEntry is null)
        {
            return string.Empty;
        }

        using var documentStream = documentEntry.Open();
        var xml = ReadStreamAsText(documentStream);
        if (string.IsNullOrWhiteSpace(xml))
        {
            return string.Empty;
        }

        var text = Regex.Replace(xml, "<[^>]+>", " ");
        return NormalizeExtractedText(WebUtility.HtmlDecode(text));
    }

    private static string ExtractPdfText(IFormFile file)
    {
        var pdfPigText = ExtractPdfTextWithPdfPig(file);
        if (!string.IsNullOrWhiteSpace(pdfPigText))
        {
            return pdfPigText;
        }

        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        var fragments = new List<string>();
        var latinText = Encoding.Latin1.GetString(bytes);

        fragments.AddRange(ExtractPdfTextFragments(latinText));

        foreach (var streamText in ExtractPdfStreams(bytes))
        {
            fragments.AddRange(ExtractPdfTextFragments(streamText));
        }

        var combined = string.Join(Environment.NewLine, fragments
            .Where(fragment => !string.IsNullOrWhiteSpace(fragment))
            .Distinct(StringComparer.OrdinalIgnoreCase));

        return NormalizeExtractedText(combined);
    }

    private static string ExtractPdfTextWithPdfPig(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var document = PdfDocument.Open(stream);

            var pageText = document.GetPages()
                .Select(page => page.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text));

            return NormalizeExtractedText(string.Join(Environment.NewLine, pageText));
        }
        catch
        {
            return string.Empty;
        }
    }

    private static IEnumerable<string> ExtractPdfStreams(byte[] bytes)
    {
        var streams = new List<string>();
        var streamMarker = Encoding.ASCII.GetBytes("stream");
        var endStreamMarker = Encoding.ASCII.GetBytes("endstream");
        var searchIndex = 0;

        while (searchIndex < bytes.Length)
        {
            var streamIndex = FindSequence(bytes, streamMarker, searchIndex);
            if (streamIndex < 0)
            {
                break;
            }

            var dataStart = streamIndex + streamMarker.Length;
            while (dataStart < bytes.Length && (bytes[dataStart] == '\r' || bytes[dataStart] == '\n'))
            {
                dataStart++;
            }

            var endIndex = FindSequence(bytes, endStreamMarker, dataStart);
            if (endIndex < 0 || endIndex <= dataStart)
            {
                break;
            }

            var chunkLength = endIndex - dataStart;
            if (chunkLength > 0)
            {
                var chunk = new byte[chunkLength];
                Array.Copy(bytes, dataStart, chunk, 0, chunkLength);

                var text = TryDecompressPdfStream(chunk);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    streams.Add(text);
                }
            }

            searchIndex = endIndex + endStreamMarker.Length;
        }

        return streams;
    }

    private static string TryDecompressPdfStream(byte[] chunk)
    {
        try
        {
            using var input = new MemoryStream(chunk);
            using var zlibStream = new ZLibStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            zlibStream.CopyTo(output);
            return Encoding.Latin1.GetString(output.ToArray());
        }
        catch
        {
            return string.Empty;
        }
    }

    private static IEnumerable<string> ExtractPdfTextFragments(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Array.Empty<string>();
        }

        var fragments = new List<string>();

        foreach (Match match in Regex.Matches(source, @"\((?<text>(?:\\.|[^\\()]){3,})\)"))
        {
            var decoded = DecodePdfLiteral(match.Groups["text"].Value);
            if (!string.IsNullOrWhiteSpace(decoded))
            {
                fragments.Add(decoded);
            }
        }

        foreach (Match match in Regex.Matches(source, @"<(?<hex>(?:[A-Fa-f0-9]{2}\s*){6,})>"))
        {
            var decoded = DecodePdfHex(match.Groups["hex"].Value);
            if (!string.IsNullOrWhiteSpace(decoded))
            {
                fragments.Add(decoded);
            }
        }

        foreach (Match match in Regex.Matches(source, @"[A-Za-z][A-Za-z0-9,\.\-+#/() ]{8,}"))
        {
            var value = match.Value.Trim();
            if (value.Contains(' '))
            {
                fragments.Add(value);
            }
        }

        return fragments;
    }

    private static string DecodePdfLiteral(string value)
    {
        var decoded = value
            .Replace(@"\(", "(")
            .Replace(@"\)", ")")
            .Replace(@"\n", " ")
            .Replace(@"\r", " ")
            .Replace(@"\t", " ")
            .Replace(@"\\", @"\");

        return NormalizeExtractedText(decoded);
    }

    private static string DecodePdfHex(string hexValue)
    {
        var cleaned = Regex.Replace(hexValue, @"\s+", string.Empty);
        if (cleaned.Length < 2 || cleaned.Length % 2 != 0)
        {
            return string.Empty;
        }

        try
        {
            var bytes = Convert.FromHexString(cleaned);
            return NormalizeExtractedText(Encoding.UTF8.GetString(bytes));
        }
        catch
        {
            return string.Empty;
        }
    }

    private static int FindSequence(byte[] source, byte[] sequence, int startIndex)
    {
        for (var index = startIndex; index <= source.Length - sequence.Length; index++)
        {
            var matched = true;
            for (var offset = 0; offset < sequence.Length; offset++)
            {
                if (source[index + offset] != sequence[offset])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return index;
            }
        }

        return -1;
    }

    private static string ReadStreamAsText(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        return NormalizeExtractedText(reader.ReadToEnd());
    }

    private static string NormalizeExtractedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = WebUtility.HtmlDecode(text).Replace('\0', ' ');
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
        return normalized.Length > 25000 ? normalized[..25000] : normalized;
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
        RankingCandidateInput RankingInput,
        string? ExtractionWarning)
    {
        public IReadOnlyCollection<string> DetectedSkills => RankingInput.Skills;
        public IReadOnlyCollection<string> ProjectKeywords => RankingInput.ProjectKeywords;
        public string RawText => RankingInput.RawText;
        public bool HasWeakExtraction => !string.IsNullOrWhiteSpace(ExtractionWarning);
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "and", "the", "with", "for", "this", "that", "from", "your", "have", "has",
        "will", "into", "our", "role", "job", "you", "are", "using", "build",
        "candidate", "candidates", "analysis", "ranking", "platform", "system",
        "senior", "developer", "engineer", "scalable", "backend", "apis", "summary",
        "remote", "onsite", "hybrid"
    };

    private static readonly string[] KnownTechnicalTerms =
    [
        "C#", "ASP.NET Core", "ASP.NET", "SQL", "Azure", "React", "Node.js",
        "MongoDB", "REST API", "Microservices", "Docker", "Kubernetes",
        "JavaScript", "TypeScript", "Python", "Machine Learning", "NLP",
        "SQL Server", "Entity Framework", "Blazor", "Git", "CI/CD"
    ];

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
