using AI.CVScreening.Api.Models.Documents;

namespace AI.CVScreening.Api.Models.Uploads;

public sealed class ScreeningBatchUploadResponse
{
    public Guid BatchId { get; set; }
    public UploadedDocumentResultDto JobDescription { get; set; } = new();
    public IReadOnlyCollection<UploadedDocumentResultDto> CandidateDocuments { get; set; } = Array.Empty<UploadedDocumentResultDto>();
    public string Message { get; set; } = string.Empty;
}
