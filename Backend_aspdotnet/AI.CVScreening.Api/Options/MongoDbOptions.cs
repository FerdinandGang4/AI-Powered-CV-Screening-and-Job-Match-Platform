namespace AI.CVScreening.Api.Options;

public sealed class MongoDbOptions
{
    public bool UseMongoDocumentStore { get; set; }
    public string ConnectionString { get; set; } = "mongodb://mongo:27017";
    public string DatabaseName { get; set; } = "cvscreening";
    public string ScreeningSubmissionsCollection { get; set; } = "screening_submissions";
}
