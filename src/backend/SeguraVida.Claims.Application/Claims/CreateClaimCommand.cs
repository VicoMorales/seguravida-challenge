using MediatR;
using SeguraVida.Claims.Domain.Claims;

namespace SeguraVida.Claims.Application.Claims;

public sealed record CreateClaimCommand(
    Guid PolicyId,
    string Type,
    DateOnly IncidentDate,
    DateOnly ReportedDate,
    decimal ClaimedAmount,
    string Description,
    string CreatedBy) : IRequest<Guid>;

internal static class ClaimTypeParser
{
    public static ClaimType Parse(string value)
    {
        return value.Equals("PROPERTY_DAMAGE", StringComparison.OrdinalIgnoreCase)
            ? ClaimType.PropertyDamage
            : Enum.Parse<ClaimType>(value, true);
    }
}
