using AI.CVScreening.Api.Models.Evaluations;
using AI.CVScreening.Api.Models.Uploads;

namespace AI.CVScreening.Api.Services;

public interface IScreeningService
{
    Task<ScreeningBatchUploadResponse> CreateBatchAsync(ScreeningBatchUploadRequest request, CancellationToken cancellationToken = default);
    RankingReportDto? GetRankingReportByBatchId(Guid batchId);
    RankingReportDto? GetRankingReport(Guid jobPostingId);
}
