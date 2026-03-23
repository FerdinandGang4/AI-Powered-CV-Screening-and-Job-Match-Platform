namespace AI.CVScreening.Api.Models.Evaluations;

public sealed class MatchExplanationDto
{
    public Guid Id { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
