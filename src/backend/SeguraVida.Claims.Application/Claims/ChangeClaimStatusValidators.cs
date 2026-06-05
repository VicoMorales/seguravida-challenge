using FluentValidation;

namespace SeguraVida.Claims.Application.Claims;

public sealed class ChangeClaimStatusCommandValidator : AbstractValidator<ChangeClaimStatusCommand>
{
    public ChangeClaimStatusCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ChangedBy).NotEmpty();
    }
}

public sealed class ApproveClaimCommandValidator : AbstractValidator<ApproveClaimCommand>
{
    public ApproveClaimCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ApprovedAmount).GreaterThan(0);
        RuleFor(command => command.PeritajeNotes).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.ChangedBy).NotEmpty();
    }
}

public sealed class RejectClaimCommandValidator : AbstractValidator<RejectClaimCommand>
{
    public RejectClaimCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.PeritajeNotes).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.ChangedBy).NotEmpty();
    }
}

public sealed class PayClaimCommandValidator : AbstractValidator<PayClaimCommand>
{
    public PayClaimCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ChangedBy).NotEmpty();
    }
}
