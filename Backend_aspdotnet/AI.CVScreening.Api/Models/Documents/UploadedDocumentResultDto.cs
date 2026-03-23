using AI.CVScreening.Api.Models.Shared;

namespace AI.CVScreening.Api.Models.Documents;

public sealed class UploadedDocumentResultDto
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DocumentProcessingStatus ProcessingStatus { get; set; }
}
