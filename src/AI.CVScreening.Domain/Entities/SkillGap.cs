using AI.CVScreening.Domain.Enums;

namespace AI.CVScreening.Domain.Entities;

public class SkillGap
{
    public Guid Id { get; set; }
    public Guid CandidateEvaluationId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string GapReason { get; set; } = string.Empty;
    public GapSeverity Severity { get; set; }

    public CandidateEvaluation? CandidateEvaluation { get; set; }
}
