using SeguraVida.Claims.Application.Common;
using MediatR;

namespace SeguraVida.Claims.Application.Auth;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IMockUserRepository _users;
    private readonly IAuthTokenService _tokens;

    public LoginCommandHandler(IMockUserRepository users, IAuthTokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.FindByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException("Mock user was not found.");

        return new LoginResponse(_tokens.CreateToken(user), user.Email, user.Role, user.DisplayName);
    }
}
