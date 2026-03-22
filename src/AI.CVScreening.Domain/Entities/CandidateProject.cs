namespace AI.CVScreening.Domain.Entities;

public class CandidateProject
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public string BusinessDomain { get; set; } = string.Empty;

    public Candidate? Candidate { get; set; }
}
