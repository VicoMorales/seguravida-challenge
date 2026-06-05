using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Infrastructure.Persistence.Configurations;

internal static class DbEnumConversion
{
    public static string ToDatabase(PolicyBranch branch) => branch.ToString().ToUpperInvariant();

    public static PolicyBranch ToPolicyBranch(string value) => Enum.Parse<PolicyBranch>(value, true);

    public static string ToDatabase(PolicyStatus status) => status.ToString().ToUpperInvariant();

    public static PolicyStatus ToPolicyStatus(string value) => Enum.Parse<PolicyStatus>(value, true);

    public static string ToDatabase(ClaimType type)
    {
        return type switch
        {
            ClaimType.PropertyDamage => "PROPERTY_DAMAGE",
            _ => type.ToString().ToUpperInvariant()
        };
    }

    public static ClaimType ToClaimType(string value)
    {
        return value.Equals("PROPERTY_DAMAGE", StringComparison.OrdinalIgnoreCase)
            ? ClaimType.PropertyDamage
            : Enum.Parse<ClaimType>(value, true);
    }

    public static string ToDatabase(ClaimStatus status)
    {
        return status switch
        {
            ClaimStatus.UnderReview => "UNDER_REVIEW",
            _ => status.ToString().ToUpperInvariant()
        };
    }

    public static ClaimStatus ToClaimStatus(string value)
    {
        return value.Equals("UNDER_REVIEW", StringComparison.OrdinalIgnoreCase)
            ? ClaimStatus.UnderReview
            : Enum.Parse<ClaimStatus>(value, true);
    }
}
