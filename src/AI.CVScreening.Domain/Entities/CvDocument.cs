namespace AI.CVScreening.Domain.Entities;

public class CvDocument
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public string ParsingStatus { get; set; } = string.Empty;

    public Candidate? Candidate { get; set; }
}
