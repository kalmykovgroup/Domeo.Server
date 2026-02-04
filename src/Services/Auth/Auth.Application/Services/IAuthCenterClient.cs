namespace Auth.Application.Services;

public interface IAuthCenterClient
{
    Task<AuthCenterTokenResponse?> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
    Task<AuthCenterTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
