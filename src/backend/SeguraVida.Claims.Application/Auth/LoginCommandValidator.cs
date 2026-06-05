using FluentValidation;

namespace SeguraVida.Claims.Application.Auth;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}
