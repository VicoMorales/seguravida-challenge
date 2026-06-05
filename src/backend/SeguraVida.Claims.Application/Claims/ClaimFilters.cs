namespace SeguraVida.Claims.Application.Claims;

public sealed record ClaimFilters(
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    string? Branch,
    DateOnly? FromDate,
    DateOnly? ToDate);
