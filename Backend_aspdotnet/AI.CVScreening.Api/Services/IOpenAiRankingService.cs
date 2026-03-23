using AI.CVScreening.Api.Models.JobPostings;

namespace AI.CVScreening.Api.Services;

public interface IOpenAiRankingService
{
    bool IsConfigured { get; }
    Task<IReadOnlyCollection<AiRankedCandidate>> RankCandidatesAsync(
        JobPostingDetailDto jobPosting,
        string jobDescriptionText,
        IReadOnlyCollection<RankingCandidateInput> candidates,
        CancellationToken cancellationToken = default);
}
