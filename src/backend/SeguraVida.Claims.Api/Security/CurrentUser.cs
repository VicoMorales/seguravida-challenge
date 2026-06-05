using System.Security.Claims;

namespace SeguraVida.Claims.Api.Security;

public static class CurrentUser
{
    public static string UserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? "anonymous";
    }
}
