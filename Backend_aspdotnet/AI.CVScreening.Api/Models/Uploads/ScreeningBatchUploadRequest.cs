using System.ComponentModel.DataAnnotations;

namespace AI.CVScreening.Api.Models.Uploads;

public sealed class ScreeningBatchUploadRequest
{
    public Guid? JobPostingId { get; set; }

    [Required]
    public IFormFile? JobDescriptionFile { get; set; }

    [Required]
    [MinLength(1)]
    public List<CandidateCvUploadItem> CandidateCvs { get; set; } = [];
}
