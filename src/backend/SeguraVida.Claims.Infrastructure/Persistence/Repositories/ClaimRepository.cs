using Microsoft.EntityFrameworkCore;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Domain.Claims;

namespace SeguraVida.Claims.Infrastructure.Persistence.Repositories;

public sealed class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _dbContext;

    public ClaimRepository(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        return _dbContext.Claims
            .Include(claim => claim.StatusHistory)
            .FirstOrDefaultAsync(claim => claim.Id == claimId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Claim>> FindByPolicyAndIncidentDateAsync(
        Guid policyId,
        DateOnly incidentDate,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Claims
            .Where(claim => claim.PolicyId == policyId && claim.IncidentDate == incidentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        await _dbContext.Claims.AddAsync(claim, cancellationToken);
    }
}
