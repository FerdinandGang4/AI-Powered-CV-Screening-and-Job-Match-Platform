using AI.CVScreening.Api.Models.Candidates;
using AI.CVScreening.Api.Models.Documents;
using AI.CVScreening.Api.Models.Shared;

namespace AI.CVScreening.Api.Services;

public sealed class InMemoryCandidateService : ICandidateService
{
    private readonly List<CandidateProfileDto> _candidates =
    [
        new()
        {
            Id = Guid.NewGuid(),
            FullName = "Amina Hassan",
            Email = "amina.hassan@example.com",
            Phone = "+1-555-1001",
            TotalYearsExperience = 5,
            CurrentLocation = "Chicago, IL",
            CvDocument = new CvDocumentDto
            {
                Id = Guid.NewGuid(),
                FileName = "amina-hassan-cv.pdf",
                ContentType = "application/pdf",
                StoragePath = "/storage/cvs/amina-hassan-cv.pdf",
                RawText = "Senior backend engineer with ASP.NET Core, SQL, Azure and REST API experience.",
                UploadedAtUtc = DateTime.UtcNow.AddDays(-2),
                ProcessingStatus = DocumentProcessingStatus.Evaluated
            },
            Skills =
            [
                new() { Id = Guid.NewGuid(), SkillName = "C#", ProficiencyLevel = "Advanced", YearsUsed = 5, LastUsedYear = DateTime.UtcNow.Year },
                new() { Id = Guid.NewGuid(), SkillName = "ASP.NET Core", ProficiencyLevel = "Advanced", YearsUsed = 4, LastUsedYear = DateTime.UtcNow.Year },
                new() { Id = Guid.NewGuid(), SkillName = "SQL", ProficiencyLevel = "Intermediate", YearsUsed = 4, LastUsedYear = DateTime.UtcNow.Year }
            ],
            WorkExperiences =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    CompanyName = "Nexa Systems",
                    RoleTitle = "Backend Developer",
                    StartDate = DateTime.UtcNow.AddYears(-5),
                    EndDate = null,
                    Summary = "Built internal and public APIs for enterprise workflows."
                }
            ],
            Projects =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    ProjectName = "Recruitment API Modernization",
                    Description = "Migrated legacy hiring workflows to modern REST APIs.",
                    Technologies = "C#, ASP.NET Core, SQL Server, Azure",
                    BusinessDomain = "HR Tech"
                }
            ],
            EducationRecords =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Institution = "University of Illinois",
                    Degree = "BSc",
                    FieldOfStudy = "Computer Science",
                    GraduationYear = DateTime.UtcNow.Year - 6
                }
            ]
        },
        new()
        {
            Id = Guid.NewGuid(),
            FullName = "David Mensah",
            Email = "david.mensah@example.com",
            Phone = "+1-555-1002",
            TotalYearsExperience = 3,
            CurrentLocation = "Austin, TX",
            CvDocument = new CvDocumentDto
            {
                Id = Guid.NewGuid(),
                FileName = "david-mensah-cv.pdf",
                ContentType = "application/pdf",
                StoragePath = "/storage/cvs/david-mensah-cv.pdf",
                RawText = "Full stack developer with React, Node.js, ASP.NET APIs and MongoDB.",
                UploadedAtUtc = DateTime.UtcNow.AddDays(-1),
                ProcessingStatus = DocumentProcessingStatus.Evaluated
            },
            Skills =
            [
                new() { Id = Guid.NewGuid(), SkillName = "React", ProficiencyLevel = "Advanced", YearsUsed = 3, LastUsedYear = DateTime.UtcNow.Year },
                new() { Id = Guid.NewGuid(), SkillName = "ASP.NET Core", ProficiencyLevel = "Intermediate", YearsUsed = 2, LastUsedYear = DateTime.UtcNow.Year },
                new() { Id = Guid.NewGuid(), SkillName = "MongoDB", ProficiencyLevel = "Intermediate", YearsUsed = 2, LastUsedYear = DateTime.UtcNow.Year }
            ],
            WorkExperiences =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    CompanyName = "Orbit Labs",
                    RoleTitle = "Software Engineer",
                    StartDate = DateTime.UtcNow.AddYears(-3),
                    EndDate = null,
                    Summary = "Worked on frontend dashboards and internal APIs."
                }
            ]
        }
    ];

    public IReadOnlyCollection<CandidateSummaryDto> GetAll()
    {
        return _candidates
            .Select(candidate => new CandidateSummaryDto
            {
                Id = candidate.Id,
                FullName = candidate.FullName,
                Email = candidate.Email,
                Phone = candidate.Phone,
                TotalYearsExperience = candidate.TotalYearsExperience,
                CurrentLocation = candidate.CurrentLocation,
                Skills = candidate.Skills
            })
            .ToArray();
    }

    public CandidateProfileDto? GetById(Guid id)
    {
        return _candidates.FirstOrDefault(candidate => candidate.Id == id);
    }
}
