using MediatR;
using Microsoft.Extensions.Logging;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Domain.Common;

namespace SeguraVida.Claims.Application.Claims;

public sealed class CreateClaimCommandHandler : IRequestHandler<CreateClaimCommand, Guid>
{
    private readonly IPolicyRepository _policies;
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;
    private readonly ILogger<CreateClaimCommandHandler> _logger;

    public CreateClaimCommandHandler(
        IPolicyRepository policies,
        IClaimRepository claims,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
        ILogger<CreateClaimCommandHandler> logger)
    {
        _policies = policies;
        _claims = claims;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        var policy = await _policies.GetByIdAsync(request.PolicyId, cancellationToken)
            ?? throw new DomainException("Policy was not found.");

        var existingClaims = await _claims.FindByPolicyAndIncidentDateAsync(
            request.PolicyId,
            request.IncidentDate,
            cancellationToken);

        if (DuplicateClaimPolicy.IsDuplicate(request.PolicyId, request.IncidentDate, request.Description, existingClaims))
        {
            throw new DomainException("A similar claim already exists for the same policy and incident date.");
        }

        var now = _clock.GetUtcNow();
        var claim = Claim.Report(
            policy,
            $"CLM-{now:yyyyMMddHHmmss}",
            ClaimTypeParser.Parse(request.Type),
            request.IncidentDate,
            request.ReportedDate,
            request.Description,
            request.ClaimedAmount,
            request.CreatedBy,
            now);

        await _claims.AddAsync(claim, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("ClaimCreated {EventType} {ClaimId} {PolicyId} {UserId}", "ClaimCreated", claim.Id, claim.PolicyId, request.CreatedBy);

        return claim.Id;
    }
}
