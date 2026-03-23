using AI.CVScreening.Api.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace AI.CVScreening.Api.Models.JobPostings;

public sealed class CreateJobRequirementRequest
{
    [Required]
    public RequirementType RequirementType { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public RequirementPriority Priority { get; set; }

    [Range(0, 100)]
    public decimal Weight { get; set; }

    public bool IsMandatory { get; set; }
}
