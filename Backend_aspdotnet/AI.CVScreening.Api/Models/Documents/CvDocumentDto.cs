using AI.CVScreening.Api.Models.Shared;

namespace AI.CVScreening.Api.Models.Documents;

public sealed class CvDocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public DocumentProcessingStatus ProcessingStatus { get; set; }
}
