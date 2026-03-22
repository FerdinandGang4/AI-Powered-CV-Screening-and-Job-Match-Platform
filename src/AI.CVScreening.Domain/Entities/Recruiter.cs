namespace AI.CVScreening.Domain.Entities;

public class Recruiter
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}
