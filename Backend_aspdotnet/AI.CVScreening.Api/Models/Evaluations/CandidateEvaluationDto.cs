using AI.CVScreening.Api.Models.Candidates;
using AI.CVScreening.Api.Models.JobPostings;

namespace AI.CVScreening.Api.Models.Evaluations;

public sealed class CandidateEvaluationDto
{
    public Guid Id { get; set; }
    public CandidateSummaryDto Candidate { get; set; } = new();
    public JobPostingSummaryDto JobPosting { get; set; } = new();
    public decimal OverallScore { get; set; }
    public decimal SkillScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal ProjectScore { get; set; }
    public decimal EducationScore { get; set; }
    public decimal SemanticScore { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public DateTime EvaluatedAtUtc { get; set; }
    public MatchExplanationDto? Explanation { get; set; }
    public IReadOnlyCollection<SkillGapDto> SkillGaps { get; set; } = Array.Empty<SkillGapDto>();
}
