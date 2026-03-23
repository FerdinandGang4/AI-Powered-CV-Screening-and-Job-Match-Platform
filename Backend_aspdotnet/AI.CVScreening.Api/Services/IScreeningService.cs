using AI.CVScreening.Api.Models.Evaluations;
using AI.CVScreening.Api.Models.Uploads;

namespace AI.CVScreening.Api.Services;

public interface IScreeningService
{
    ScreeningBatchUploadResponse CreateBatch(ScreeningBatchUploadRequest request);
    RankingReportDto? GetRankingReportByBatchId(Guid batchId);
    RankingReportDto? GetRankingReport(Guid jobPostingId);
}
