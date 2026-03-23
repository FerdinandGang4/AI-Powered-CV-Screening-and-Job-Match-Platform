using AI.CVScreening.Api.Models.Shared;

namespace AI.CVScreening.Api.Models.JobPostings;

public sealed class JobRequirementDto
{
    public Guid Id { get; set; }
    public RequirementType RequirementType { get; set; }
    public string Name { get; set; } = string.Empty;
    public RequirementPriority Priority { get; set; }
    public decimal Weight { get; set; }
    public bool IsMandatory { get; set; }
}
