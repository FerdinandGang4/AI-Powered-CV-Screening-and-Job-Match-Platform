namespace AI.CVScreening.Domain.Entities;

public class CandidateEvaluation
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public Guid JobPostingId { get; set; }
    public decimal OverallScore { get; set; }
    public decimal SkillScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal ProjectScore { get; set; }
    public decimal EducationScore { get; set; }
    public decimal SemanticScore { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public DateTime EvaluatedAtUtc { get; set; }

    public Candidate? Candidate { get; set; }
    public JobPosting? JobPosting { get; set; }
    public ICollection<SkillGap> SkillGaps { get; set; } = new List<SkillGap>();
    public MatchExplanation? MatchExplanation { get; set; }
}
