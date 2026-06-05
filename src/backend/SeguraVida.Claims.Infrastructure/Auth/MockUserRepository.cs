using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SeguraVida.Claims.Application.Auth;
using SeguraVida.Claims.Infrastructure.Persistence;

namespace SeguraVida.Claims.Infrastructure.Auth;

public sealed class MockUserRepository : IMockUserRepository
{
    private readonly ClaimsDbContext _dbContext;

    public MockUserRepository(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MockUserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT TOP 1 user_id, email, role, display_name
            FROM MOCK_USER
            WHERE email = @Email
            """;
        command.Parameters.Add(new SqlParameter("@Email", email));

        await _dbContext.Database.OpenConnectionAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new MockUserDto(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
    }
}
