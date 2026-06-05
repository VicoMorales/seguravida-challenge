using Microsoft.EntityFrameworkCore;
using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Domain.Parties;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Infrastructure.Persistence;

public sealed class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options)
    {
    }

    public DbSet<InsuredParty> InsuredParties => Set<InsuredParty>();
    public DbSet<InsurancePolicy> Policies => Set<InsurancePolicy>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimStatusHistory> ClaimStatusHistory => Set<ClaimStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
    }
}
