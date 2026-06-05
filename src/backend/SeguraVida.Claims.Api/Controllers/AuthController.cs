using MediatR;
using Microsoft.AspNetCore.Mvc;
using SeguraVida.Claims.Api.Contracts.Auth;
using SeguraVida.Claims.Application.Auth;

namespace SeguraVida.Claims.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new LoginCommand(request.Email), cancellationToken));
    }
}
