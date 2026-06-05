using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeguraVida.Claims.Api.Contracts.Claims;
using SeguraVida.Claims.Api.Security;
using SeguraVida.Claims.Application.Claims;
using SeguraVida.Claims.Application.Common;

namespace SeguraVida.Claims.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.Operator},{Roles.Adjuster},{Roles.Auditor}")]
[Route("api/claims")]
public sealed class ClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClaimListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ClaimListItemDto>>> GetClaims(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? branch = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetClaimsQuery(page, pageSize, search, status, branch, fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClaimDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClaimDetailDto>> GetClaimDetail(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClaimDetailQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Operator)]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<ActionResult> CreateClaim(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var claimId = await _mediator.Send(
            new CreateClaimCommand(
                request.PolicyId,
                request.Type,
                request.IncidentDate,
                request.ReportedDate,
                request.ClaimedAmount,
                request.Description,
                CurrentUser.UserId(User)),
            cancellationToken);

        return CreatedAtAction(nameof(GetClaimDetail), new { id = claimId }, new { claimId });
    }

    [HttpPost("{id:guid}/start-review")]
    [Authorize(Roles = Roles.Adjuster)]
    public async Task<IActionResult> StartReview(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ChangeClaimStatusCommand(id, CurrentUser.UserId(User)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = Roles.Adjuster)]
    public async Task<IActionResult> Approve(Guid id, ApproveClaimRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveClaimCommand(id, request.ApprovedAmount, request.PeritajeNotes, CurrentUser.UserId(User)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = Roles.Adjuster)]
    public async Task<IActionResult> Reject(Guid id, RejectClaimRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RejectClaimCommand(id, request.PeritajeNotes, CurrentUser.UserId(User)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = Roles.Adjuster)]
    public async Task<IActionResult> Pay(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new PayClaimCommand(id, CurrentUser.UserId(User)), cancellationToken);
        return NoContent();
    }
}
