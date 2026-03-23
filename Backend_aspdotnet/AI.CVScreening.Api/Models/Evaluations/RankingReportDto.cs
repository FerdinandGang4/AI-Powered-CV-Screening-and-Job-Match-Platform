using AI.CVScreening.Api.Models.JobPostings;

namespace AI.CVScreening.Api.Models.Evaluations;

public sealed class RankingReportDto
{
    public Guid Id { get; set; }
    public JobPostingSummaryDto JobPosting { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; }
    public bool AiUsed { get; set; }
    public int TotalCandidates { get; set; }
    public Guid? TopCandidateId { get; set; }
    public IReadOnlyCollection<CandidateEvaluationDto> RankedCandidates { get; set; } = Array.Empty<CandidateEvaluationDto>();
}
