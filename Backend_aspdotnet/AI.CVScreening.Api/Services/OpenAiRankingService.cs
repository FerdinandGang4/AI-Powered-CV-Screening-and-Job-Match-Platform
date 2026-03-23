using AI.CVScreening.Api.Models.JobPostings;
using AI.CVScreening.Api.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AI.CVScreening.Api.Services;

public sealed class OpenAiRankingService(
    HttpClient httpClient,
    IOptions<OpenAiOptions> options) : IOpenAiRankingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly OpenAiOptions _options = options.Value;

    public bool IsConfigured => _options.UseAiRanking && !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<IReadOnlyCollection<AiRankedCandidate>> RankCandidatesAsync(
        JobPostingDetailDto jobPosting,
        string jobDescriptionText,
        IReadOnlyCollection<RankingCandidateInput> candidates,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("OpenAI ranking is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = BuildRequestPayload(jobPosting, jobDescriptionText, candidates);
        request.Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI ranking request failed with status {(int)response.StatusCode}: {responseText}");
        }

        var document = JsonNode.Parse(responseText)
            ?? throw new InvalidOperationException("OpenAI ranking response was empty.");

        var outputJson = document["output_text"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(outputJson))
        {
            outputJson = document["output"]?.AsArray()
                .SelectMany(outputItem => outputItem?["content"]?.AsArray() ?? [])
                .Select(content => content?["text"]?.GetValue<string>())
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

        if (string.IsNullOrWhiteSpace(outputJson))
        {
            throw new InvalidOperationException("OpenAI ranking response did not contain structured output text.");
        }

        var rankingResponse = JsonSerializer.Deserialize<OpenAiRankingResponse>(outputJson, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI ranking response could not be parsed.");

        return rankingResponse.RankedCandidates
            .Select(candidate => new AiRankedCandidate(
                candidate.CandidateId,
                candidate.OverallScore,
                candidate.Summary,
                candidate.Strengths,
                candidate.Weaknesses,
                candidate.MissingSkills))
            .ToArray();
    }

    private JsonObject BuildRequestPayload(
        JobPostingDetailDto jobPosting,
        string jobDescriptionText,
        IReadOnlyCollection<RankingCandidateInput> candidates)
    {
        return new JsonObject
        {
            ["model"] = _options.Model,
            ["input"] = new JsonArray
            {
                BuildMessage("system",
                    """
                    You are an expert technical recruiting assistant. Rank candidates for a job posting.
                    Return only structured JSON that follows the schema exactly.
                    Be strict about missing mandatory skills or insufficient experience.
                    """),
                BuildMessage("user", BuildPrompt(jobPosting, jobDescriptionText, candidates))
            },
            ["text"] = new JsonObject
            {
                ["format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["name"] = "candidate_ranking",
                    ["strict"] = true,
                    ["schema"] = BuildResponseSchema()
                }
            }
        };
    }

    private static JsonObject BuildMessage(string role, string text)
    {
        return new JsonObject
        {
            ["role"] = role,
            ["content"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "input_text",
                    ["text"] = text
                }
            }
        };
    }

    private static string BuildPrompt(
        JobPostingDetailDto jobPosting,
        string jobDescriptionText,
        IReadOnlyCollection<RankingCandidateInput> candidates)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Job Title: {jobPosting.Title}");
        builder.AppendLine($"Department: {jobPosting.Department}");
        builder.AppendLine($"Location: {jobPosting.Location}");
        builder.AppendLine($"Minimum Years Experience: {jobPosting.MinimumYearsExperience}");
        builder.AppendLine($"Job Description: {jobDescriptionText}");
        builder.AppendLine("Requirements:");

        foreach (var requirement in jobPosting.Requirements)
        {
            builder.AppendLine(
                $"- {requirement.Name} | Mandatory: {requirement.IsMandatory} | Priority: {requirement.Priority} | Weight: {requirement.Weight}");
        }

        builder.AppendLine("Candidates:");
        foreach (var candidate in candidates)
        {
            builder.AppendLine($"CandidateId: {candidate.CandidateId}");
            builder.AppendLine($"Name: {candidate.FullName}");
            builder.AppendLine($"Email: {candidate.Email}");
            builder.AppendLine($"YearsExperience: {candidate.TotalYearsExperience}");
            builder.AppendLine($"Skills: {string.Join(", ", candidate.Skills)}");
            builder.AppendLine($"Projects: {string.Join(", ", candidate.ProjectKeywords)}");
            builder.AppendLine($"CV Text: {candidate.RawText}");
            builder.AppendLine("---");
        }

        builder.AppendLine("Rank every candidate and give realistic scores from 0 to 100.");
        builder.AppendLine("Missing mandatory skills must materially lower the score.");
        builder.AppendLine("Use concise recruiter-friendly explanations.");
        return builder.ToString();
    }

    private static JsonObject BuildResponseSchema()
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["properties"] = new JsonObject
            {
                ["rankedCandidates"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "object",
                        ["additionalProperties"] = false,
                        ["properties"] = new JsonObject
                        {
                            ["candidateId"] = new JsonObject { ["type"] = "string" },
                            ["overallScore"] = new JsonObject { ["type"] = "number" },
                            ["summary"] = new JsonObject { ["type"] = "string" },
                            ["strengths"] = new JsonObject { ["type"] = "string" },
                            ["weaknesses"] = new JsonObject { ["type"] = "string" },
                            ["missingSkills"] = new JsonObject
                            {
                                ["type"] = "array",
                                ["items"] = new JsonObject { ["type"] = "string" }
                            }
                        },
                        ["required"] = new JsonArray("candidateId", "overallScore", "summary", "strengths", "weaknesses", "missingSkills")
                    }
                }
            },
            ["required"] = new JsonArray("rankedCandidates")
        };
    }

    private sealed class OpenAiRankingResponse
    {
        public List<OpenAiRankedCandidate> RankedCandidates { get; set; } = [];
    }

    private sealed class OpenAiRankedCandidate
    {
        public Guid CandidateId { get; set; }
        public decimal OverallScore { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public IReadOnlyCollection<string> MissingSkills { get; set; } = Array.Empty<string>();
    }
}
