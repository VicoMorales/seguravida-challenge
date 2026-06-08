using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Application.Abstractions;

public interface IPolicyRepository
{
    Task<InsurancePolicy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken);

    Task<InsurancePolicy?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken);
}
