namespace AI.CVScreening.Domain.Entities;

public class JobPosting
{
    public Guid Id { get; set; }
    public Guid RecruiterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string DescriptionText { get; set; } = string.Empty;
    public int MinimumYearsExperience { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public Recruiter? Recruiter { get; set; }
    public ICollection<JobRequirement> Requirements { get; set; } = new List<JobRequirement>();
    public ICollection<CandidateEvaluation> CandidateEvaluations { get; set; } = new List<CandidateEvaluation>();
    public ICollection<RankingReport> RankingReports { get; set; } = new List<RankingReport>();
}
