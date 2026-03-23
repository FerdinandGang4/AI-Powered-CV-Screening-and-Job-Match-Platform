namespace AI.CVScreening.Api.Models.Candidates;

public sealed class WorkExperienceDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Summary { get; set; } = string.Empty;
}
