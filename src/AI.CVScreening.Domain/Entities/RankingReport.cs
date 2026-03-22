namespace AI.CVScreening.Domain.Entities;

public class RankingReport
{
    public Guid Id { get; set; }
    public Guid JobPostingId { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public int TotalCandidates { get; set; }
    public Guid? TopCandidateId { get; set; }

    public JobPosting? JobPosting { get; set; }
    public ICollection<CandidateEvaluation> RankedEvaluations { get; set; } = new List<CandidateEvaluation>();
}
