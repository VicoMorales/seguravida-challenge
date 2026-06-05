namespace SeguraVida.Claims.Application.Claims;

public sealed record ClaimListItemDto(
    Guid ClaimId,
    string ClaimNumber,
    string PolicyNumber,
    string Branch,
    string Type,
    string Status,
    DateOnly IncidentDate,
    DateOnly ReportedDate,
    decimal ClaimedAmount,
    decimal? ApprovedAmount);

public sealed record ClaimDetailDto(
    Guid ClaimId,
    string ClaimNumber,
    Guid PolicyId,
    string PolicyNumber,
    string Branch,
    string Type,
    string Description,
    DateOnly IncidentDate,
    DateOnly ReportedDate,
    decimal ClaimedAmount,
    decimal? ApprovedAmount,
    string Status,
    string? PeritajeNotes,
    IReadOnlyCollection<ClaimStatusHistoryDto> History);

public sealed record ClaimStatusHistoryDto(
    Guid HistoryId,
    string PreviousStatus,
    string NewStatus,
    string ChangedBy,
    DateTimeOffset ChangedAt,
    string? Reason);
