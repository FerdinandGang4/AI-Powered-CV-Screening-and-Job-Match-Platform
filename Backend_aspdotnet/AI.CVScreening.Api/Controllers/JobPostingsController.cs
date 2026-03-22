using AI.CVScreening.Api.Models.JobPostings;
using AI.CVScreening.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI.CVScreening.Api.Controllers;

public sealed class JobPostingsController(IJobPostingService jobPostingService) : BaseApiController
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<JobPostingSummaryDto>> GetAll()
    {
        return Ok(jobPostingService.GetAll());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<JobPostingSummaryDto> GetById(Guid id)
    {
        var jobPosting = jobPostingService.GetById(id);
        if (jobPosting is null)
        {
            return NotFound();
        }

        return Ok(jobPosting);
    }

    [HttpPost]
    public ActionResult<JobPostingSummaryDto> Create(CreateJobPostingRequest request)
    {
        var createdJobPosting = jobPostingService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = createdJobPosting.Id }, createdJobPosting);
    }
}
