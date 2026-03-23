using AI.CVScreening.Api.Models.Candidates;
using AI.CVScreening.Api.Models.Evaluations;
using AI.CVScreening.Api.Models.JobPostings;

namespace AI.CVScreening.Api.Services;

public sealed class AppMemoryStore
{
    public List<JobPostingDetailDto> JobPostings { get; } = [];
    public List<CandidateProfileDto> Candidates { get; } = [];
    public Dictionary<Guid, RankingReportDto> ReportsByBatchId { get; } = [];
    public Dictionary<Guid, Guid> BatchToJobPostingMap { get; } = [];
}
