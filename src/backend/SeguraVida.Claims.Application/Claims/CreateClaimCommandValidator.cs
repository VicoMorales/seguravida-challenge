using FluentValidation;

namespace SeguraVida.Claims.Application.Claims;

public sealed class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator()
    {
        RuleFor(command => command.PolicyNumber).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Type).NotEmpty();
        RuleFor(command => command.Description).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.ClaimedAmount).GreaterThan(0);
        RuleFor(command => command.CreatedBy).NotEmpty();
        RuleFor(command => command)
            .Must(command => command.IncidentDate <= command.ReportedDate)
            .WithMessage("Incident date cannot be after report date.");
    }
}
