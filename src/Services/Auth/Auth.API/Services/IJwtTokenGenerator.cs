namespace Auth.API.Services;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(
        Guid userId,
        string email,
        string name,
        string role);

    string GenerateRefreshToken();

    DateTime GetAccessTokenExpiration();
    TimeSpan GetRefreshTokenLifetime();
}
