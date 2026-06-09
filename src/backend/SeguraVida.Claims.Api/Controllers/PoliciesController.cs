using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeguraVida.Claims.Api.Security;
using SeguraVida.Claims.Application.Policies;

namespace SeguraVida.Claims.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Operator)]
[Route("api/policies")]
public sealed class PoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PoliciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{policyNumber}")]
    [ProducesResponseType(typeof(PolicyLookupDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PolicyLookupDto>> GetByNumber(string policyNumber, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetPolicyByNumberQuery(policyNumber), cancellationToken));
    }
}
