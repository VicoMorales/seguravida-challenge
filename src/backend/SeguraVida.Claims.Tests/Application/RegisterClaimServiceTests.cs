using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Application.Claims;
using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Domain.Common;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Tests.Application;

public sealed class RegisterClaimServiceTests
{
    [Fact]
    public async Task RegisterAsync_rejects_duplicate_claim_for_same_policy_date_and_similar_description()
    {
        var policy = new InsurancePolicy(
            Guid.NewGuid(),
            "POL-001",
            "AUTO",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            10000m);

        var existingClaim = Claim.Report(
            policy,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 5),
            "Choque lateral del vehiculo asegurado",
            1000m,
            "operator",
            DateTimeOffset.UtcNow);

        var service = new RegisterClaimService(
            new StubPolicyRepository(policy),
            new StubClaimRepository([existingClaim]),
            new StubUnitOfWork(),
            TimeProvider.System);

        var command = new RegisterClaimCommand(
            policy.Id,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 5),
            "choque lateral del vehículo asegurado",
            1000m,
            "operator");

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.RegisterAsync(command, CancellationToken.None));

        Assert.Contains("similar claim", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_persists_claim_when_business_rules_pass()
    {
        var policy = new InsurancePolicy(
            Guid.NewGuid(),
            "POL-001",
            "AUTO",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            10000m);
        var claims = new StubClaimRepository([]);
        var unitOfWork = new StubUnitOfWork();
        var service = new RegisterClaimService(new StubPolicyRepository(policy), claims, unitOfWork, TimeProvider.System);

        var claimId = await service.RegisterAsync(
            new RegisterClaimCommand(
                policy.Id,
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 5),
                "Vehicle collision",
                1000m,
                "operator"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, claimId);
        Assert.Single(claims.AddedClaims);
        Assert.Single(claims.AddedClaims[0].StatusHistory);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    private sealed class StubPolicyRepository : IPolicyRepository
    {
        private readonly InsurancePolicy _policy;

        public StubPolicyRepository(InsurancePolicy policy)
        {
            _policy = policy;
        }

        public Task<InsurancePolicy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken)
        {
            return Task.FromResult<InsurancePolicy?>(_policy.Id == policyId ? _policy : null);
        }
    }

    private sealed class StubClaimRepository : IClaimRepository
    {
        private readonly IReadOnlyCollection<Claim> _existingClaims;

        public StubClaimRepository(IReadOnlyCollection<Claim> existingClaims)
        {
            _existingClaims = existingClaims;
        }

        public List<Claim> AddedClaims { get; } = [];

        public Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_existingClaims.Concat(AddedClaims).FirstOrDefault(claim => claim.Id == claimId));
        }

        public Task<IReadOnlyCollection<Claim>> FindByPolicyAndIncidentDateAsync(
            Guid policyId,
            DateOnly incidentDate,
            CancellationToken cancellationToken)
        {
            var matches = _existingClaims
                .Where(claim => claim.PolicyId == policyId && claim.IncidentDate == incidentDate)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<Claim>>(matches);
        }

        public Task AddAsync(Claim claim, CancellationToken cancellationToken)
        {
            AddedClaims.Add(claim);
            return Task.CompletedTask;
        }
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public int SaveCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCalls++;
            return Task.FromResult(1);
        }
    }
}
