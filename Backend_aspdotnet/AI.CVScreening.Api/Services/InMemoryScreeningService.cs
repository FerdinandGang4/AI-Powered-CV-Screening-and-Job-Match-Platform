using AI.CVScreening.Api.Models.Candidates;
using AI.CVScreening.Api.Models.Documents;
using AI.CVScreening.Api.Models.Evaluations;
using AI.CVScreening.Api.Models.JobPostings;
using AI.CVScreening.Api.Models.Shared;
using AI.CVScreening.Api.Models.Uploads;

namespace AI.CVScreening.Api.Services;

public sealed class InMemoryScreeningService(
    IJobPostingService jobPostingService,
    ICandidateService candidateService,
    AppMemoryStore store) : IScreeningService
{
    public ScreeningBatchUploadResponse CreateBatch(ScreeningBatchUploadRequest request)
    {
        var jobPostingId = request.JobPostingId
            ?? jobPostingService.GetAll().FirstOrDefault()?.Id
            ?? throw new InvalidOperationException("No job posting is available for candidate screening.");

        var rankingReport = GetRankingReport(jobPostingId)
            ?? throw new InvalidOperationException("Unable to generate a ranking report for the selected job posting.");

        var batchId = Guid.NewGuid();
        store.ReportsByBatchId[batchId] = rankingReport;
        store.BatchToJobPostingMap[batchId] = jobPostingId;

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
        var jobPosting = jobPostingService.GetById(jobPostingId);
        if (jobPosting is null)
        {
            return null;
        }

        var rankedCandidates = candidateService.GetAll()
            .Select(candidate => BuildEvaluation(candidate, jobPosting))
            .OrderByDescending(evaluation => evaluation.OverallScore)
            .ToArray();

        return new RankingReportDto
        {
            Id = Guid.NewGuid(),
            JobPosting = MapToSummary(jobPosting),
            GeneratedAtUtc = DateTime.UtcNow,
            TotalCandidates = rankedCandidates.Length,
            TopCandidateId = rankedCandidates.FirstOrDefault()?.Candidate.Id,
            RankedCandidates = rankedCandidates
        };
    }

    private static CandidateEvaluationDto BuildEvaluation(CandidateSummaryDto candidate, JobPostingDetailDto jobPosting)
    {
        var candidateSkillNames = candidate.Skills
            .Select(skill => skill.SkillName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var matchedRequirements = jobPosting.Requirements
            .Where(requirement => candidateSkillNames.Contains(requirement.Name))
            .ToArray();

        var missingRequirements = jobPosting.Requirements
            .Where(requirement => !candidateSkillNames.Contains(requirement.Name))
            .ToArray();

        var skillScore = matchedRequirements.Sum(requirement => requirement.Weight);
        var experienceScore = candidate.TotalYearsExperience >= jobPosting.MinimumYearsExperience ? 20 : 10;
        var projectScore = 15;
        var educationScore = 10;
        var semanticScore = matchedRequirements.Length > 0 ? 12 : 6;
        var overallScore = Math.Min(100, skillScore + experienceScore + projectScore + educationScore + semanticScore);

        return new CandidateEvaluationDto
        {
            Id = Guid.NewGuid(),
            Candidate = candidate,
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
                Summary = $"{candidate.FullName} matched {matchedRequirements.Length} out of {jobPosting.Requirements.Count} tracked requirements.",
                Strengths = matchedRequirements.Length == 0
                    ? "No direct skill matches were found in the current sample profile."
                    : $"Matched skills: {string.Join(", ", matchedRequirements.Select(requirement => requirement.Name))}.",
                Weaknesses = missingRequirements.Length == 0
                    ? "No major requirement gaps were detected."
                    : $"Missing or weak areas: {string.Join(", ", missingRequirements.Select(requirement => requirement.Name))}.",
                Notes = $"Candidate has {candidate.TotalYearsExperience} years of experience for a role requiring {jobPosting.MinimumYearsExperience}+ years."
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
