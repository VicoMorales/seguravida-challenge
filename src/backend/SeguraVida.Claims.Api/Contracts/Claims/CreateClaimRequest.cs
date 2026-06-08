namespace SeguraVida.Claims.Api.Contracts.Claims;

public sealed record CreateClaimRequest(
    string PolicyNumber,
    string Type,
    DateOnly IncidentDate,
    DateOnly ReportedDate,
    decimal ClaimedAmount,
    string Description);
