using SeguraVida.Claims.Application.Policies;

namespace SeguraVida.Claims.Application.Abstractions;

public interface IPolicyReadRepository
{
    Task<PolicyLookupDto?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken);
}
