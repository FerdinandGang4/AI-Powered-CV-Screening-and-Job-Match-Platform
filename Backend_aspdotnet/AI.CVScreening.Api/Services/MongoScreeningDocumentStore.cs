using AI.CVScreening.Api.Models.Persistence;
using AI.CVScreening.Api.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AI.CVScreening.Api.Services;

public sealed class MongoScreeningDocumentStore : IScreeningDocumentStore
{
    private readonly IMongoCollection<ScreeningSubmissionDocument> _collection;

    public MongoScreeningDocumentStore(IOptions<MongoDbOptions> options)
    {
        var mongoOptions = options.Value;
        var client = new MongoClient(mongoOptions.ConnectionString);
        var database = client.GetDatabase(mongoOptions.DatabaseName);
        _collection = database.GetCollection<ScreeningSubmissionDocument>(mongoOptions.ScreeningSubmissionsCollection);
    }

    public Task SaveSubmissionAsync(ScreeningSubmissionDocument document, CancellationToken cancellationToken = default)
    {
        return _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}
