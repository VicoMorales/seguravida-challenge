using MediatR;
using SeguraVida.Claims.Application.Common;

namespace SeguraVida.Claims.Application.Claims;

public sealed record GetClaimsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    string? Branch,
    DateOnly? FromDate,
    DateOnly? ToDate) : IRequest<PagedResult<ClaimListItemDto>>;
