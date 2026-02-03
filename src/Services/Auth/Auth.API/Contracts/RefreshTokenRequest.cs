using System.Text.Json.Serialization;

namespace Auth.API.Contracts;

public sealed record RefreshTokenRequest(string RefreshToken)
{
    [JsonConstructor]
    public RefreshTokenRequest() : this("") { }
}
