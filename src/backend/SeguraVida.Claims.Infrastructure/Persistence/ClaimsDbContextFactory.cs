using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SeguraVida.Claims.Infrastructure.Persistence;

public sealed class ClaimsDbContextFactory : IDesignTimeDbContextFactory<ClaimsDbContext>
{
    public ClaimsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SEGURAVIDA_DB")
            ?? "Server=localhost,1433;Database=SeguraVidaClaims;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ClaimsDbContext(options);
    }
}
