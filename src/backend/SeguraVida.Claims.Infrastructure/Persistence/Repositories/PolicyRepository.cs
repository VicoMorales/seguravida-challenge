using Microsoft.EntityFrameworkCore;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Infrastructure.Persistence.Repositories;

public sealed class PolicyRepository : IPolicyRepository
{
    private readonly ClaimsDbContext _dbContext;

    public PolicyRepository(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<InsurancePolicy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return _dbContext.Policies.FirstOrDefaultAsync(policy => policy.Id == policyId, cancellationToken);
    }

    public Task<InsurancePolicy?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken)
    {
        return _dbContext.Policies.FirstOrDefaultAsync(policy => policy.PolicyNumber == policyNumber, cancellationToken);
    }
}
