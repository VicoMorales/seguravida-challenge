namespace SeguraVida.Claims.Application.Policies;

public sealed record PolicySummaryDto(
    Guid PolicyId,
    string PolicyNumber,
    string Branch,
    decimal Premium,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal InsuredAmount,
    string Status);

public sealed record InsuredPartySummaryDto(
    string FullName,
    string MaskedDocumentId,
    string MaskedEmail);

public sealed record PolicyLookupDto(
    PolicySummaryDto Policy,
    InsuredPartySummaryDto InsuredParty);
