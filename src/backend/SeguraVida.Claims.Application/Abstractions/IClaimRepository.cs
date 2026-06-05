using SeguraVida.Claims.Domain.Claims;

namespace SeguraVida.Claims.Application.Abstractions;

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Claim>> FindByPolicyAndIncidentDateAsync(
        Guid policyId,
        DateOnly incidentDate,
        CancellationToken cancellationToken);

    Task AddAsync(Claim claim, CancellationToken cancellationToken);
}
