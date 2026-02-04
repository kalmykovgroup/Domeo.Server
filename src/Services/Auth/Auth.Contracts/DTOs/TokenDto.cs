using System.Text.Json.Serialization;

namespace Auth.Contracts.DTOs;

public sealed record TokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt)
{
    [JsonConstructor]
    public TokenDto() : this("", "", DateTime.MinValue) { }
}
