namespace SeguraVida.Claims.Application.Auth;

public interface IAuthTokenService
{
    string CreateToken(MockUserDto user);
}
