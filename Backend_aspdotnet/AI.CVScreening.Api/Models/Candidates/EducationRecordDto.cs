namespace AI.CVScreening.Api.Models.Candidates;

public sealed class EducationRecordDto
{
    public Guid Id { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
}
