namespace SeguraVida.Claims.Domain.Claims;

public static class DuplicateClaimPolicy
{
    public static bool IsDuplicate(
        Guid policyId,
        DateOnly incidentDate,
        string description,
        IEnumerable<Claim> existingClaims)
    {
        var normalizedDescription = ClaimTextNormalizer.Normalize(description);

        return existingClaims.Any(existing =>
            existing.PolicyId == policyId &&
            existing.IncidentDate == incidentDate &&
            AreSimilar(existing.NormalizedDescription, normalizedDescription));
    }

    private static bool AreSimilar(string left, string right)
    {
        if (left == right)
        {
            return true;
        }

        if (left.Length == 0 || right.Length == 0)
        {
            return false;
        }

        var shorter = left.Length <= right.Length ? left : right;
        var longer = left.Length > right.Length ? left : right;

        return longer.Contains(shorter, StringComparison.Ordinal) ||
               Similarity(shorter, longer) >= 0.85m;
    }

    private static decimal Similarity(string left, string right)
    {
        var distance = LevenshteinDistance(left, right);
        var maxLength = Math.Max(left.Length, right.Length);

        return maxLength == 0 ? 1m : 1m - ((decimal)distance / maxLength);
    }

    private static int LevenshteinDistance(string left, string right)
    {
        var costs = new int[right.Length + 1];

        for (var j = 0; j < costs.Length; j++)
        {
            costs[j] = j;
        }

        for (var i = 1; i <= left.Length; i++)
        {
            costs[0] = i;
            var previousDiagonal = i - 1;

            for (var j = 1; j <= right.Length; j++)
            {
                var previousAbove = costs[j];
                var substitutionCost = left[i - 1] == right[j - 1] ? previousDiagonal : previousDiagonal + 1;
                costs[j] = Math.Min(Math.Min(costs[j] + 1, costs[j - 1] + 1), substitutionCost);
                previousDiagonal = previousAbove;
            }
        }

        return costs[^1];
    }
}
