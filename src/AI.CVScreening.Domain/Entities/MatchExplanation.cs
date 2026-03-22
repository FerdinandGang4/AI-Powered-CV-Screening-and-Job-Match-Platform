namespace AI.CVScreening.Domain.Entities;

public class MatchExplanation
{
    public Guid Id { get; set; }
    public Guid CandidateEvaluationId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public CandidateEvaluation? CandidateEvaluation { get; set; }
}
