using System.Text.Json.Serialization;

namespace Auth.API.Contracts;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword)
{
    [JsonConstructor]
    public ChangePasswordRequest() : this("", "") { }
}
