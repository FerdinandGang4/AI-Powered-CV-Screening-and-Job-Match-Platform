using AI.CVScreening.Api.Models.JobPostings;

namespace AI.CVScreening.Api.Services;

public sealed class InMemoryJobPostingService : IJobPostingService
{
    private readonly List<JobPostingSummaryDto> _jobPostings =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Senior ASP.NET Developer",
            Department = "Engineering",
            DescriptionText = "Build scalable backend APIs for CV analysis and candidate ranking.",
            MinimumYearsExperience = 4,
            Location = "Remote",
            CreatedAtUtc = DateTime.UtcNow
        }
    ];

    public IReadOnlyCollection<JobPostingSummaryDto> GetAll()
    {
        return _jobPostings
            .OrderByDescending(jobPosting => jobPosting.CreatedAtUtc)
            .ToArray();
    }

    public JobPostingSummaryDto? GetById(Guid id)
    {
        return _jobPostings.FirstOrDefault(jobPosting => jobPosting.Id == id);
    }

    public JobPostingSummaryDto Create(CreateJobPostingRequest request)
    {
        var jobPosting = new JobPostingSummaryDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Department = request.Department,
            DescriptionText = request.DescriptionText,
            MinimumYearsExperience = request.MinimumYearsExperience,
            Location = request.Location,
            CreatedAtUtc = DateTime.UtcNow
        };

        _jobPostings.Add(jobPosting);
        return jobPosting;
    }
}
