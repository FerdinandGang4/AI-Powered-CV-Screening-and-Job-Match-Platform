using AI.CVScreening.Api.Models.Candidates;
using AI.CVScreening.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI.CVScreening.Api.Controllers;

public sealed class CandidatesController(ICandidateService candidateService) : BaseApiController
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<CandidateSummaryDto>> GetAll()
    {
        return Ok(candidateService.GetAll());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<CandidateProfileDto> GetById(Guid id)
    {
        var candidate = candidateService.GetById(id);
        if (candidate is null)
        {
            return NotFound();
        }

        return Ok(candidate);
    }
}
