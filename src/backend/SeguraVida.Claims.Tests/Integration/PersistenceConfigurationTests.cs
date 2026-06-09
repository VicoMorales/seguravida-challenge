using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Infrastructure.Persistence;

namespace SeguraVida.Claims.Tests.Integration;

public sealed class PersistenceConfigurationTests
{
    [Fact]
    public void Claim_status_history_id_is_generated_by_domain()
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase("metadata-check")
            .Options;

        using var dbContext = new ClaimsDbContext(options);

        var entityType = dbContext.Model.FindEntityType(typeof(ClaimStatusHistory));
        var idProperty = entityType!.FindProperty(nameof(ClaimStatusHistory.Id));

        idProperty!.ValueGenerated.Should().Be(ValueGenerated.Never);
    }
}
