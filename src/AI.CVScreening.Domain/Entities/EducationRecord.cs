namespace AI.CVScreening.Domain.Entities;

public class EducationRecord
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public int GraduationYear { get; set; }

    public Candidate? Candidate { get; set; }
}
