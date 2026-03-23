namespace AI.CVScreening.Api.Models.Candidates;

public sealed class CandidateProjectDto
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public string BusinessDomain { get; set; } = string.Empty;
}
