namespace SeguraVida.Claims.Application.Reports;

public interface IClaimsReportRepository
{
    Task<IReadOnlyCollection<ClaimSummaryRow>> GetClaimsSummaryAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken);
}
