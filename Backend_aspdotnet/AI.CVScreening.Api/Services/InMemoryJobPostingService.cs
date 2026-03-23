using AI.CVScreening.Api.Models.JobPostings;
using AI.CVScreening.Api.Models.Shared;

namespace AI.CVScreening.Api.Services;

public sealed class InMemoryJobPostingService : IJobPostingService
{
    private readonly AppMemoryStore _store;

    public InMemoryJobPostingService(AppMemoryStore store)
    {
        _store = store;

        if (_store.JobPostings.Count == 0)
        {
            _store.JobPostings.Add(new JobPostingDetailDto
            {
                Id = Guid.NewGuid(),
                Title = "Software Engineer (.NET Core / SQL Server)",
                Department = "Engineering",
                DescriptionText = """
                    This role focuses on designing, building, and supporting modern, scalable software solutions using C#, .NET Core, and SQL Server. It includes contributing to new development efforts as well as enhancing and evolving existing systems, with an emphasis on performance, maintainability, and overall quality.
                    The position contributes to Digitech's technology roadmap, supports modernization efforts, and helps improve operational efficiency and user experience.
                    Key responsibilities include backend development with C# and .NET Core, SQL Server database development, API integration, Agile/Scrum participation, code reviews, troubleshooting production issues, collaboration with Product, QA, DevOps, and Architecture, technical documentation, and supporting CI/CD pipelines with version control and automated testing.
                    Minimum qualifications include a Bachelor's degree in Computer Science or a related field, 3-5 years of professional software engineering experience, strong hands-on experience with C#, .NET Core, and SQL Server, solid knowledge of object-oriented programming, data structures, and design patterns, experience building or consuming APIs, familiarity with Agile methodologies, strong problem-solving ability, and the ability to work effectively in a remote environment.
                    """,
                MinimumYearsExperience = 3,
                Location = "Remote",
                CreatedAtUtc = DateTime.UtcNow,
                Requirements =
                [
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
                        RequirementType = RequirementType.Skill,
                        Name = ".NET Core",
                        Priority = RequirementPriority.Critical,
                        Weight = 28,
                        IsMandatory = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RequirementType = RequirementType.Skill,
                        Name = "SQL Server",
                        Priority = RequirementPriority.High,
                        Weight = 24,
                        IsMandatory = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RequirementType = RequirementType.Skill,
                        Name = "REST API",
                        Priority = RequirementPriority.High,
                        Weight = 16,
                        IsMandatory = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RequirementType = RequirementType.Skill,
                        Name = "Agile",
                        Priority = RequirementPriority.Medium,
                        Weight = 10,
                        IsMandatory = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RequirementType = RequirementType.Project,
                        Name = "CI/CD",
                        Priority = RequirementPriority.Medium,
                        Weight = 8,
                        IsMandatory = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RequirementType = RequirementType.Project,
                        Name = "Code Reviews",
                        Priority = RequirementPriority.Medium,
                        Weight = 6,
                        IsMandatory = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RequirementType = RequirementType.Project,
                        Name = "Troubleshooting",
                        Priority = RequirementPriority.Medium,
                        Weight = 6,
                        IsMandatory = true
                    }
                ]
            });
        }
    }

    public IReadOnlyCollection<JobPostingSummaryDto> GetAll()
    {
        return _store.JobPostings
            .OrderByDescending(jobPosting => jobPosting.CreatedAtUtc)
            .Select(MapToSummary)
            .ToArray();
    }

    public JobPostingDetailDto? GetById(Guid id)
    {
        return _store.JobPostings.FirstOrDefault(jobPosting => jobPosting.Id == id);
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

        _store.JobPostings.Add(jobPosting);
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
