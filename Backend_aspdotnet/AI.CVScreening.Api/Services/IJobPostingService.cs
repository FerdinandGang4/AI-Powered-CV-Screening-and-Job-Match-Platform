using AI.CVScreening.Api.Models.JobPostings;

namespace AI.CVScreening.Api.Services;

public interface IJobPostingService
{
    IReadOnlyCollection<JobPostingSummaryDto> GetAll();
    JobPostingSummaryDto? GetById(Guid id);
    JobPostingSummaryDto Create(CreateJobPostingRequest request);
}
