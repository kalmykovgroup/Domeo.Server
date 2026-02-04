using System.Net.Http.Json;
using System.Text.Json;
using Auth.Application.Services;
using Auth.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services;

public class AuthCenterClient : IAuthCenterClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthCenterClient> _logger;

    public AuthCenterClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AuthCenterClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthCenterTokenResponse?> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var clientId = _configuration["AuthCenter:ClientId"] ?? "domeo";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = clientId
        });

        try
        {
            var response = await _httpClient.PostAsync("/token", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to exchange code: {StatusCode} - {Error}",
                    response.StatusCode, error);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AuthCenterTokenResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code");
            return null;
        }
    }

    public async Task<AuthCenterTokenResponse?> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var clientId = _configuration["AuthCenter:ClientId"] ?? "domeo";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = clientId
        });

        try
        {
            var response = await _httpClient.PostAsync("/token", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to refresh token: {StatusCode} - {Error}",
                    response.StatusCode, error);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AuthCenterTokenResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return null;
        }
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken
            });

            await _httpClient.PostAsync("/logout", content, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error revoking token at Auth Center");
        }
    }

    public async Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var clientId = _configuration["AuthCenter:ClientId"] ?? "domeo";

        try
        {
            var response = await _httpClient.GetAsync($"/users/{userId}?client_id={clientId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info: {StatusCode}", response.StatusCode);
                return null;
            }

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);

            var root = doc.RootElement;

            if (!root.TryGetProperty("id", out var idProp) || !Guid.TryParse(idProp.GetString(), out var id))
                return null;

            return new User
            {
                Id = id,
                Role = root.TryGetProperty("role", out var roleProp) ? roleProp.GetString() ?? string.Empty : string.Empty,
                Email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null,
                Name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info from Auth Center");
            return null;
        }
    }
}
