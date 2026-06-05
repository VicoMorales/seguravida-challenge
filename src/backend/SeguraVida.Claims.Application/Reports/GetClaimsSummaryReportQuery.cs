using MediatR;

namespace SeguraVida.Claims.Application.Reports;

public sealed record GetClaimsSummaryReportQuery(DateOnly? FromDate, DateOnly? ToDate) : IRequest<IReadOnlyCollection<ClaimSummaryRow>>;
