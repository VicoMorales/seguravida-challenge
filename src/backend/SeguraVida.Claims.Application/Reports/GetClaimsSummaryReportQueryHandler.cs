using MediatR;

namespace SeguraVida.Claims.Application.Reports;

public sealed class GetClaimsSummaryReportQueryHandler : IRequestHandler<GetClaimsSummaryReportQuery, IReadOnlyCollection<ClaimSummaryRow>>
{
    private readonly IClaimsReportRepository _reports;

    public GetClaimsSummaryReportQueryHandler(IClaimsReportRepository reports)
    {
        _reports = reports;
    }

    public Task<IReadOnlyCollection<ClaimSummaryRow>> Handle(GetClaimsSummaryReportQuery request, CancellationToken cancellationToken)
    {
        return _reports.GetClaimsSummaryAsync(request.FromDate, request.ToDate, cancellationToken);
    }
}
