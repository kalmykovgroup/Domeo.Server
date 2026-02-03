using System.Text.Json.Serialization;

namespace Auth.API.Contracts;

/// <summary>
/// Request to exchange authorization code for tokens
/// </summary>
public sealed record CallbackRequest(string Code, string RedirectUri)
{
    [JsonConstructor]
    public CallbackRequest() : this("", "") { }
}
