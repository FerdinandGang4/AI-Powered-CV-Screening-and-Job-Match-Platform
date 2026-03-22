namespace AI.CVScreening.Domain.Entities;

public class WorkExperience
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Summary { get; set; } = string.Empty;

    public Candidate? Candidate { get; set; }
}
