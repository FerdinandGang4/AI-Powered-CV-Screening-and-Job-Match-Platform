using AI.CVScreening.Api.Models.JobPostings;
using AI.CVScreening.Api.Models.Shared;

namespace AI.CVScreening.Api.Services;

public sealed class InMemoryJobPostingService : IJobPostingService
{
    private readonly List<JobPostingDetailDto> _jobPostings =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Senior ASP.NET Developer",
            Department = "Engineering",
            DescriptionText = "Build scalable backend APIs for CV analysis and candidate ranking.",
            MinimumYearsExperience = 4,
            Location = "Remote",
            CreatedAtUtc = DateTime.UtcNow,
            Requirements =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    RequirementType = RequirementType.Skill,
                    Name = "ASP.NET Core",
                    Priority = RequirementPriority.Critical,
                    Weight = 35,
                    IsMandatory = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    RequirementType = RequirementType.Skill,
                    Name = "C#",
                    Priority = RequirementPriority.Critical,
                    Weight = 30,
                    IsMandatory = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    RequirementType = RequirementType.Experience,
                    Name = "Backend API Development",
                    Priority = RequirementPriority.High,
                    Weight = 20,
                    IsMandatory = true
                }
            ]
        }
    ];

    public IReadOnlyCollection<JobPostingSummaryDto> GetAll()
    {
        return _jobPostings
            .OrderByDescending(jobPosting => jobPosting.CreatedAtUtc)
            .Select(MapToSummary)
            .ToArray();
    }

    public JobPostingDetailDto? GetById(Guid id)
    {
        return _jobPostings.FirstOrDefault(jobPosting => jobPosting.Id == id);
    }

    public JobPostingDetailDto Create(CreateJobPostingRequest request)
    {
        var jobPosting = new JobPostingDetailDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Department = request.Department,
            DescriptionText = request.DescriptionText,
            MinimumYearsExperience = request.MinimumYearsExperience,
            Location = request.Location,
            CreatedAtUtc = DateTime.UtcNow,
            Requirements = request.Requirements
                .Select(requirement => new JobRequirementDto
                {
                    Id = Guid.NewGuid(),
                    RequirementType = requirement.RequirementType,
                    Name = requirement.Name,
                    Priority = requirement.Priority,
                    Weight = requirement.Weight,
                    IsMandatory = requirement.IsMandatory
                })
                .ToArray()
        };

        _jobPostings.Add(jobPosting);
        return jobPosting;
    }

    private static JobPostingSummaryDto MapToSummary(JobPostingDetailDto detail)
    {
        return new JobPostingSummaryDto
        {
            Id = detail.Id,
            Title = detail.Title,
            Department = detail.Department,
            DescriptionText = detail.DescriptionText,
            MinimumYearsExperience = detail.MinimumYearsExperience,
            Location = detail.Location,
            CreatedAtUtc = detail.CreatedAtUtc
        };
    }
}
