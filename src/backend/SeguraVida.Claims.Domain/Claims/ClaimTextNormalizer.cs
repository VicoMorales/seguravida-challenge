using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SeguraVida.Claims.Domain.Claims;

public static partial class ClaimTextNormalizer
{
    public static string Normalize(string value)
    {
        var normalized = value.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return MultipleSpaces().Replace(builder.ToString().Normalize(NormalizationForm.FormC), " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpaces();
}
