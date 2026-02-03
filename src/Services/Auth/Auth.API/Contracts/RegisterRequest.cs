using System.Text.Json.Serialization;

namespace Auth.API.Contracts;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName)
{
    [JsonConstructor]
    public RegisterRequest() : this("", "", "", "") { }
}
