using Microsoft.EntityFrameworkCore;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Application.Policies;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Infrastructure.Persistence.Repositories;

public sealed class PolicyReadRepository : IPolicyReadRepository
{
    private readonly ClaimsDbContext _dbContext;

    public PolicyReadRepository(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PolicyLookupDto?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken)
    {
        var result = await (
            from policy in _dbContext.Policies.AsNoTracking()
            join party in _dbContext.InsuredParties.AsNoTracking() on policy.HolderId equals party.Id
            where policy.PolicyNumber == policyNumber
            select new { policy, party })
            .FirstOrDefaultAsync(cancellationToken);

        return result is null
            ? null
            : new PolicyLookupDto(
                new PolicySummaryDto(
                    result.policy.Id,
                    result.policy.PolicyNumber,
                    ToApi(result.policy.Branch),
                    result.policy.Premium,
                    result.policy.ValidFrom,
                    result.policy.ValidTo,
                    result.policy.InsuredAmount,
                    ToApi(result.policy.Status)),
                new InsuredPartySummaryDto(
                    result.party.FullName,
                    MaskDocument(result.party.DocumentId),
                    MaskEmail(result.party.Email)));
    }

    private static string ToApi(PolicyBranch branch) => branch.ToString().ToUpperInvariant();

    private static string ToApi(PolicyStatus status) => status.ToString().ToUpperInvariant();

    private static string MaskDocument(string documentId)
    {
        if (documentId.Length <= 4)
        {
            return new string('*', documentId.Length);
        }

        return $"{documentId[..3]}****{documentId[^2..]}";
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@', StringComparison.Ordinal);

        if (atIndex <= 1)
        {
            return "***";
        }

        var name = email[..atIndex];
        var domain = email[atIndex..];
        return $"{name[0]}***{domain}";
    }
}
