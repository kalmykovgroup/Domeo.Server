using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Services;

public sealed class InternalApiKeyDelegatingHandler : DelegatingHandler
{
    private readonly string? _apiKey;

    public InternalApiKeyDelegatingHandler(IConfiguration configuration)
    {
        _apiKey = configuration["InternalApi:Key"];
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_apiKey))
        {
            request.Headers.Add("X-Internal-Api-Key", _apiKey);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
