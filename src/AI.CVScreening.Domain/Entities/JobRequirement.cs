using AI.CVScreening.Domain.Enums;

namespace AI.CVScreening.Domain.Entities;

public class JobRequirement
{
    public Guid Id { get; set; }
    public Guid JobPostingId { get; set; }
    public RequirementType RequirementType { get; set; }
    public string Name { get; set; } = string.Empty;
    public RequirementPriority Priority { get; set; }
    public decimal Weight { get; set; }
    public bool IsMandatory { get; set; }

    public JobPosting? JobPosting { get; set; }
}
