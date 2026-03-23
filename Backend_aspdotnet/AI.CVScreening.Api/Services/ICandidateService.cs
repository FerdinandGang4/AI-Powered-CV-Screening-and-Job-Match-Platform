using AI.CVScreening.Api.Models.Candidates;

namespace AI.CVScreening.Api.Services;

public interface ICandidateService
{
    IReadOnlyCollection<CandidateSummaryDto> GetAll();
    CandidateProfileDto? GetById(Guid id);
}
