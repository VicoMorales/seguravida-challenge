using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Domain.Common;

namespace SeguraVida.Claims.Application.Claims;

public sealed class RegisterClaimService
{
    private readonly IPolicyRepository _policies;
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;

    public RegisterClaimService(
        IPolicyRepository policies,
        IClaimRepository claims,
        IUnitOfWork unitOfWork,
        TimeProvider clock)
    {
        _policies = policies;
        _claims = claims;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Guid> RegisterAsync(RegisterClaimCommand command, CancellationToken cancellationToken)
    {
        var policy = await _policies.GetByIdAsync(command.PolicyId, cancellationToken);

        if (policy is null)
        {
            throw new DomainException("Policy was not found.");
        }

        var existingClaims = await _claims.FindByPolicyAndIncidentDateAsync(
            command.PolicyId,
            command.IncidentDate,
            cancellationToken);

        if (DuplicateClaimPolicy.IsDuplicate(command.PolicyId, command.IncidentDate, command.Description, existingClaims))
        {
            throw new DomainException("A similar claim already exists for the same policy and incident date.");
        }

        var claim = Claim.Report(
            policy,
            command.IncidentDate,
            command.ReportedAt,
            command.Description,
            command.ClaimedAmount,
            command.ReportedBy,
            _clock.GetUtcNow());

        await _claims.AddAsync(claim, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return claim.Id;
    }
}
