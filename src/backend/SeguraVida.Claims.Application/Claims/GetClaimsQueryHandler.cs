using MediatR;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Application.Common;

namespace SeguraVida.Claims.Application.Claims;

public sealed class GetClaimsQueryHandler : IRequestHandler<GetClaimsQuery, PagedResult<ClaimListItemDto>>
{
    private readonly IClaimReadRepository _claims;

    public GetClaimsQueryHandler(IClaimReadRepository claims)
    {
        _claims = claims;
    }

    public Task<PagedResult<ClaimListItemDto>> Handle(GetClaimsQuery request, CancellationToken cancellationToken)
    {
        var filters = new ClaimFilters(
            Math.Max(1, request.Page),
            Math.Clamp(request.PageSize, 1, 100),
            request.Search,
            request.Status,
            request.Branch,
            request.FromDate,
            request.ToDate);

        return _claims.SearchAsync(filters, cancellationToken);
    }
}
