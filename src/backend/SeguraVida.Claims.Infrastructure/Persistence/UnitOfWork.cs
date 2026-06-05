using SeguraVida.Claims.Application.Abstractions;

namespace SeguraVida.Claims.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ClaimsDbContext _dbContext;

    public UnitOfWork(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
