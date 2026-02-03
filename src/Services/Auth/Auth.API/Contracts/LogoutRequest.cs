using System.Text.Json.Serialization;

namespace Auth.API.Contracts;

public sealed record LogoutRequest(string RefreshToken)
{
    [JsonConstructor]
    public LogoutRequest() : this("") { }
}
