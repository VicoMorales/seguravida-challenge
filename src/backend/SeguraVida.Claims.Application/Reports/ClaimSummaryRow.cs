namespace SeguraVida.Claims.Application.Reports;

public sealed record ClaimSummaryRow(
    string Branch,
    string Status,
    int TotalClaims,
    decimal PaidAmount);
