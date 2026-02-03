using System.Text.Json.Serialization;

namespace Domeo.Shared.Contracts.DTOs;

public sealed record TokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt)
{
    [JsonConstructor]
    public TokenDto() : this("", "", DateTime.MinValue) { }
}
