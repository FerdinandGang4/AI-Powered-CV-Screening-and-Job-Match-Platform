namespace AI.CVScreening.Api.Models.Candidates;

public sealed class CandidateSkillDto
{
    public Guid Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string ProficiencyLevel { get; set; } = string.Empty;
    public decimal YearsUsed { get; set; }
    public int LastUsedYear { get; set; }
}
