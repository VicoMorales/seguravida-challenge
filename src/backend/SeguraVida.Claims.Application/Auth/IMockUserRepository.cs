namespace SeguraVida.Claims.Application.Auth;

public interface IMockUserRepository
{
    Task<MockUserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}
