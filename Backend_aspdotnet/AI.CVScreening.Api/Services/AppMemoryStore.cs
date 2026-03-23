using AI.CVScreening.Api.Models.Candidates;
using AI.CVScreening.Api.Models.Evaluations;
using AI.CVScreening.Api.Models.JobPostings;
using AI.CVScreening.Api.Models.Auth;

namespace AI.CVScreening.Api.Services;

public sealed class AppMemoryStore
{
    public List<JobPostingDetailDto> JobPostings { get; } = [];
    public List<CandidateProfileDto> Candidates { get; } = [];
    public List<RecruiterAccountDto> RecruiterAccounts { get; } = [];
    public Dictionary<string, Guid> RecruiterSessions { get; } = new(StringComparer.Ordinal);
    public Dictionary<Guid, RankingReportDto> ReportsByBatchId { get; } = [];
    public Dictionary<Guid, Guid> BatchToJobPostingMap { get; } = [];
    public Dictionary<Guid, RankingReportDto> LatestReportsByJobPostingId { get; } = [];
}
