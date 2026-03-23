using AI.CVScreening.Api.Models.Evaluations;
using AI.CVScreening.Api.Models.Uploads;
using AI.CVScreening.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI.CVScreening.Api.Controllers;

public sealed class ScreeningController(IScreeningService screeningService) : BaseApiController
{
    [HttpPost("batches")]
    public async Task<ActionResult<ScreeningBatchUploadResponse>> CreateBatch([FromForm] ScreeningBatchUploadRequest request, CancellationToken cancellationToken)
    {
        var response = await screeningService.CreateBatchAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("reports/{jobPostingId:guid}")]
    public ActionResult<RankingReportDto> GetRankingReport(Guid jobPostingId)
    {
        var report = screeningService.GetRankingReport(jobPostingId);
        if (report is null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    [HttpGet("batches/{batchId:guid}/report")]
    public ActionResult<RankingReportDto> GetRankingReportByBatchId(Guid batchId)
    {
        var report = screeningService.GetRankingReportByBatchId(batchId);
        if (report is null)
        {
            return NotFound();
        }

        return Ok(report);
    }
}
