using AI.CVScreening.Api.Models.JobPostings;

namespace AI.CVScreening.Api.Services;

public interface IJobPostingService
{
    IReadOnlyCollection<JobPostingSummaryDto> GetAll();
    JobPostingDetailDto? GetById(Guid id);
    JobPostingDetailDto Create(CreateJobPostingRequest request);
}
