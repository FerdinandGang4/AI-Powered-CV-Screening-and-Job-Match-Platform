namespace AI.CVScreening.Api.Services;

public sealed record RankingCandidateInput(
    Guid CandidateId,
    string FullName,
    string Email,
    decimal TotalYearsExperience,
    IReadOnlyCollection<string> Skills,
    IReadOnlyCollection<string> ProjectKeywords,
    string RawText);

public sealed record AiRankedCandidate(
    Guid CandidateId,
    decimal OverallScore,
    string Summary,
    string Strengths,
    string Weaknesses,
    IReadOnlyCollection<string> MissingSkills);
