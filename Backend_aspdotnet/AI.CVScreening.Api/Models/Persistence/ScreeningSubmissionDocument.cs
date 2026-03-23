namespace AI.CVScreening.Api.Models.Persistence;

public sealed class ScreeningSubmissionDocument
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescriptionText { get; set; } = string.Empty;
    public string JobDescriptionSource { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyCollection<ScreeningSubmissionCandidateDocument> Candidates { get; set; } = Array.Empty<ScreeningSubmissionCandidateDocument>();
    public string RankingReportJson { get; set; } = string.Empty;
}

public sealed class ScreeningSubmissionCandidateDocument
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
