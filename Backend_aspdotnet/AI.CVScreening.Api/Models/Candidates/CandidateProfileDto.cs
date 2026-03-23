using AI.CVScreening.Api.Models.Documents;

namespace AI.CVScreening.Api.Models.Candidates;

public sealed class CandidateProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal TotalYearsExperience { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;
    public CvDocumentDto? CvDocument { get; set; }
    public IReadOnlyCollection<CandidateSkillDto> Skills { get; set; } = Array.Empty<CandidateSkillDto>();
    public IReadOnlyCollection<WorkExperienceDto> WorkExperiences { get; set; } = Array.Empty<WorkExperienceDto>();
    public IReadOnlyCollection<CandidateProjectDto> Projects { get; set; } = Array.Empty<CandidateProjectDto>();
    public IReadOnlyCollection<EducationRecordDto> EducationRecords { get; set; } = Array.Empty<EducationRecordDto>();
}
