namespace SeguraVida.Claims.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "SeguraVida.Claims";
    public string Audience { get; set; } = "SeguraVida.Claims.Web";
    public string Secret { get; set; } = "development-only-secret-key-change-before-production";
    public int ExpirationMinutes { get; set; } = 120;
}
