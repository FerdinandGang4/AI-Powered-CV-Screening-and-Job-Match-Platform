using AI.CVScreening.Api.Models.Shared;

namespace AI.CVScreening.Api.Models.Evaluations;

public sealed class SkillGapDto
{
    public Guid Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string GapReason { get; set; } = string.Empty;
    public GapSeverity Severity { get; set; }
}
