using AI.CVScreening.Api.Models.Persistence;

namespace AI.CVScreening.Api.Services;

public interface IScreeningDocumentStore
{
    Task SaveSubmissionAsync(ScreeningSubmissionDocument document, CancellationToken cancellationToken = default);
}
