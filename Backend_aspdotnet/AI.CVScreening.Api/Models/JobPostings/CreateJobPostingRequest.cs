using System.ComponentModel.DataAnnotations;

namespace AI.CVScreening.Api.Models.JobPostings;

public sealed class CreateJobPostingRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string DescriptionText { get; set; } = string.Empty;

    [Range(0, 50)]
    public int MinimumYearsExperience { get; set; }

    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;
}
