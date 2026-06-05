namespace SeguraVida.Claims.Application.Claims;

public sealed record RegisterClaimCommand(
    Guid PolicyId,
    DateOnly IncidentDate,
    DateOnly ReportedAt,
    string Description,
    decimal ClaimedAmount,
    string ReportedBy);
