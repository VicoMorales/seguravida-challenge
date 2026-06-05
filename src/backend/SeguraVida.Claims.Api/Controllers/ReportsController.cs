using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeguraVida.Claims.Api.Security;
using SeguraVida.Claims.Application.Reports;

namespace SeguraVida.Claims.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Auditor)]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("claims-summary")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ClaimSummaryRow>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ClaimSummaryRow>>> GetClaimsSummary(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClaimsSummaryReportQuery(fromDate, toDate), cancellationToken));
    }
}
