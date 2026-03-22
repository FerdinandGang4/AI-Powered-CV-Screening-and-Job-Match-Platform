namespace AI.CVScreening.Domain.Entities;

public class Candidate
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal TotalYearsExperience { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;

    public CvDocument? CvDocument { get; set; }
    public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    public ICollection<CandidateProject> Projects { get; set; } = new List<CandidateProject>();
    public ICollection<EducationRecord> EducationRecords { get; set; } = new List<EducationRecord>();
    public ICollection<CandidateEvaluation> CandidateEvaluations { get; set; } = new List<CandidateEvaluation>();
}
