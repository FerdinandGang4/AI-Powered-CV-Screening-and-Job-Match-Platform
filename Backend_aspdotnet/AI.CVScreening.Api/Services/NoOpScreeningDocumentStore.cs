using AI.CVScreening.Api.Models.Persistence;

namespace AI.CVScreening.Api.Services;

public sealed class NoOpScreeningDocumentStore : IScreeningDocumentStore
{
    public Task SaveSubmissionAsync(ScreeningSubmissionDocument document, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
