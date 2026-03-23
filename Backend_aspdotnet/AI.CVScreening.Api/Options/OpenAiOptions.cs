namespace AI.CVScreening.Api.Options;

public sealed class OpenAiOptions
{
    public bool UseAiRanking { get; set; } = true;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string Endpoint { get; set; } = "https://api.openai.com/v1/responses";
}
