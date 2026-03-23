using System.ComponentModel.DataAnnotations;

namespace AI.CVScreening.Api.Models.Uploads;

public sealed class ScreeningBatchUploadRequest
{
    [Required]
    public IFormFile? JobDescriptionFile { get; set; }

    [MinLength(1)]
    public IReadOnlyCollection<CandidateCvUploadItem> CandidateCvs { get; set; } = Array.Empty<CandidateCvUploadItem>();
}
