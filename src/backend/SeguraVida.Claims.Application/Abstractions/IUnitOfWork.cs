namespace SeguraVida.Claims.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
