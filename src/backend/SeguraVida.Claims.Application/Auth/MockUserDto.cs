namespace SeguraVida.Claims.Application.Auth;

public sealed record MockUserDto(Guid UserId, string Email, string Role, string DisplayName);
