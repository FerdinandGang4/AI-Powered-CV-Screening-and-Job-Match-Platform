namespace AI.CVScreening.Api.Models.JobPostings;

public sealed class JobPostingDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string DescriptionText { get; set; } = string.Empty;
    public int MinimumYearsExperience { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyCollection<JobRequirementDto> Requirements { get; set; } = Array.Empty<JobRequirementDto>();
}
