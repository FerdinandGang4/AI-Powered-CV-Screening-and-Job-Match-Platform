using System.ComponentModel.DataAnnotations;

namespace AI.CVScreening.Api.Models.Uploads;

public sealed class CandidateCvUploadItem
{
    [Required]
    [MaxLength(150)]
    public string CandidateName { get; set; } = string.Empty;

    [EmailAddress]
    public string CandidateEmail { get; set; } = string.Empty;

    [Required]
    public IFormFile? CvFile { get; set; }
}
