using Microsoft.EntityFrameworkCore;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Application.Claims;
using SeguraVida.Claims.Application.Common;
using SeguraVida.Claims.Application.Policies;
using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Infrastructure.Persistence.Repositories;

public sealed class ClaimReadRepository : IClaimReadRepository
{
    private readonly ClaimsDbContext _dbContext;

    public ClaimReadRepository(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ClaimListItemDto>> SearchAsync(ClaimFilters filters, CancellationToken cancellationToken)
    {
        var query =
            from claim in _dbContext.Claims.AsNoTracking()
            join policy in _dbContext.Policies.AsNoTracking() on claim.PolicyId equals policy.Id
            join party in _dbContext.InsuredParties.AsNoTracking() on policy.HolderId equals party.Id
            select new { claim, policy, party };

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.Trim();
            query = query.Where(row =>
                row.claim.ClaimNumber.Contains(search) ||
                row.policy.PolicyNumber.Contains(search) ||
                row.party.DocumentId.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            var status = ParseClaimStatus(filters.Status);
            query = query.Where(row => row.claim.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filters.Branch))
        {
            var branch = Enum.Parse<PolicyBranch>(filters.Branch, true);
            query = query.Where(row => row.policy.Branch == branch);
        }

        if (filters.FromDate is not null)
        {
            query = query.Where(row => row.claim.ReportedAt >= filters.FromDate);
        }

        if (filters.ToDate is not null)
        {
            query = query.Where(row => row.claim.ReportedAt <= filters.ToDate);
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(row => row.claim.ReportedAt)
            .ThenBy(row => row.claim.ClaimNumber)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);
        var items = rows
            .Select(row => new ClaimListItemDto(
                row.claim.Id,
                row.claim.ClaimNumber,
                row.policy.PolicyNumber,
                ToApi(row.policy.Branch),
                ToApi(row.claim.Type),
                ToApi(row.claim.Status),
                row.claim.IncidentDate,
                row.claim.ReportedAt,
                row.claim.ClaimedAmount,
                row.claim.ApprovedAmount))
            .ToList();

        return new PagedResult<ClaimListItemDto>(items, filters.Page, filters.PageSize, total);
    }

    public async Task<ClaimDetailDto?> GetDetailAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var result = await (
            from claim in _dbContext.Claims.AsNoTracking()
            join policy in _dbContext.Policies.AsNoTracking() on claim.PolicyId equals policy.Id
            join party in _dbContext.InsuredParties.AsNoTracking() on policy.HolderId equals party.Id
            where claim.Id == claimId
            select new { claim, policy, party })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        var history = await _dbContext.ClaimStatusHistory
            .AsNoTracking()
            .Where(item => item.ClaimId == claimId)
            .OrderBy(item => item.ChangedAt)
            .Select(item => new ClaimStatusHistoryDto(
                item.Id,
                ToApi(item.FromStatus),
                ToApi(item.ToStatus),
                item.ChangedBy,
                item.ChangedAt,
                item.Notes))
            .ToListAsync(cancellationToken);

        return new ClaimDetailDto(
            result.claim.Id,
            result.claim.ClaimNumber,
            result.policy.Id,
            result.policy.PolicyNumber,
            ToApi(result.policy.Branch),
            ToApi(result.claim.Type),
            result.claim.Description,
            result.claim.IncidentDate,
            result.claim.ReportedAt,
            result.claim.ClaimedAmount,
            result.claim.ApprovedAmount,
            ToApi(result.claim.Status),
            result.claim.AdjustmentNotes,
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
                MaskEmail(result.party.Email)),
            history);
    }

    private static ClaimStatus ParseClaimStatus(string value)
    {
        return value.Equals("UNDER_REVIEW", StringComparison.OrdinalIgnoreCase)
            ? ClaimStatus.UnderReview
            : Enum.Parse<ClaimStatus>(value, true);
    }

    private static string ToApi(PolicyBranch branch) => branch.ToString().ToUpperInvariant();

    private static string ToApi(PolicyStatus status) => status.ToString().ToUpperInvariant();

    private static string ToApi(ClaimType type)
    {
        return type == ClaimType.PropertyDamage ? "PROPERTY_DAMAGE" : type.ToString().ToUpperInvariant();
    }

    private static string ToApi(ClaimStatus status)
    {
        return status == ClaimStatus.UnderReview ? "UNDER_REVIEW" : status.ToString().ToUpperInvariant();
    }

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
