namespace AI.CVScreening.Domain.Entities;

public class CandidateSkill
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string ProficiencyLevel { get; set; } = string.Empty;
    public decimal YearsUsed { get; set; }
    public int LastUsedYear { get; set; }

    public Candidate? Candidate { get; set; }
}
