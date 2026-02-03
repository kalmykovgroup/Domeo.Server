using System.Text.Json.Serialization;

namespace Auth.API.Contracts;

public sealed record LoginRequest(string Email, string Password)
{
    [JsonConstructor]
    public LoginRequest() : this("", "") { }
}
