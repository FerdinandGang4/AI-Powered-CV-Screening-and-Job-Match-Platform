namespace AI.CVScreening.Api.Models.Shared;

public enum DocumentProcessingStatus
{
    Uploaded = 1,
    Parsing = 2,
    Parsed = 3,
    Failed = 4,
    Evaluated = 5
}
