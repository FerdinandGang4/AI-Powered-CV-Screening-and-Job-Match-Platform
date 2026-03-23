namespace AI.CVScreening.Api.Models.Candidates;

public sealed class CandidateSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal TotalYearsExperience { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;
    public IReadOnlyCollection<CandidateSkillDto> Skills { get; set; } = Array.Empty<CandidateSkillDto>();
}
