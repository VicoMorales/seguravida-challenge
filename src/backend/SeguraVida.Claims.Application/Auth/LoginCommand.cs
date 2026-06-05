using MediatR;

namespace SeguraVida.Claims.Application.Auth;

public sealed record LoginCommand(string Email) : IRequest<LoginResponse>;

public sealed record LoginResponse(string AccessToken, string Email, string Role, string DisplayName);
