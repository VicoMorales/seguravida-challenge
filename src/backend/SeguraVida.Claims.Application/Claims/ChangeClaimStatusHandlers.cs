using MediatR;
using Microsoft.Extensions.Logging;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Application.Common;

namespace SeguraVida.Claims.Application.Claims;

public sealed class ChangeClaimStatusCommandHandler : IRequestHandler<ChangeClaimStatusCommand, Unit>
{
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;
    private readonly ILogger<ChangeClaimStatusCommandHandler> _logger;

    public ChangeClaimStatusCommandHandler(
        IClaimRepository claims,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
        ILogger<ChangeClaimStatusCommandHandler> logger)
    {
        _claims = claims;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Unit> Handle(ChangeClaimStatusCommand request, CancellationToken cancellationToken)
    {
        var claim = await _claims.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException("Claim was not found.");

        claim.StartReview(request.ChangedBy, _clock.GetUtcNow(), "Review started.");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("ClaimStatusChanged {EventType} {ClaimId} {PolicyId} {UserId}", "ClaimStatusChanged", claim.Id, claim.PolicyId, request.ChangedBy);

        return Unit.Value;
    }
}

public sealed class ApproveClaimCommandHandler : IRequestHandler<ApproveClaimCommand, Unit>
{
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;
    private readonly ILogger<ApproveClaimCommandHandler> _logger;

    public ApproveClaimCommandHandler(IClaimRepository claims, IUnitOfWork unitOfWork, TimeProvider clock, ILogger<ApproveClaimCommandHandler> logger)
    {
        _claims = claims;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Unit> Handle(ApproveClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _claims.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException("Claim was not found.");

        claim.Approve(request.ApprovedAmount, request.PeritajeNotes, request.ChangedBy, _clock.GetUtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("ClaimApproved {EventType} {ClaimId} {PolicyId} {UserId}", "ClaimApproved", claim.Id, claim.PolicyId, request.ChangedBy);

        return Unit.Value;
    }
}

public sealed class RejectClaimCommandHandler : IRequestHandler<RejectClaimCommand, Unit>
{
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;
    private readonly ILogger<RejectClaimCommandHandler> _logger;

    public RejectClaimCommandHandler(IClaimRepository claims, IUnitOfWork unitOfWork, TimeProvider clock, ILogger<RejectClaimCommandHandler> logger)
    {
        _claims = claims;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Unit> Handle(RejectClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _claims.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException("Claim was not found.");

        claim.Reject(request.PeritajeNotes, request.ChangedBy, _clock.GetUtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("ClaimRejected {EventType} {ClaimId} {PolicyId} {UserId}", "ClaimRejected", claim.Id, claim.PolicyId, request.ChangedBy);

        return Unit.Value;
    }
}

public sealed class PayClaimCommandHandler : IRequestHandler<PayClaimCommand, Unit>
{
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;
    private readonly ILogger<PayClaimCommandHandler> _logger;

    public PayClaimCommandHandler(IClaimRepository claims, IUnitOfWork unitOfWork, TimeProvider clock, ILogger<PayClaimCommandHandler> logger)
    {
        _claims = claims;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Unit> Handle(PayClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _claims.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException("Claim was not found.");

        claim.MarkAsPaid(request.ChangedBy, _clock.GetUtcNow(), "Payment completed.");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("ClaimPaid {EventType} {ClaimId} {PolicyId} {UserId}", "ClaimPaid", claim.Id, claim.PolicyId, request.ChangedBy);

        return Unit.Value;
    }
}
