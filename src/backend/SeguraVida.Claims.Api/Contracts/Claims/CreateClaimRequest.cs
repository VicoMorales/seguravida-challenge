namespace SeguraVida.Claims.Api.Contracts.Claims;

public sealed record CreateClaimRequest(
    Guid PolicyId,
    string Type,
    DateOnly IncidentDate,
    DateOnly ReportedDate,
    decimal ClaimedAmount,
    string Description);
