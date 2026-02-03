using System.Text.Json.Serialization;

namespace Domeo.Shared.Contracts.DTOs;

public sealed record AuthResultDto(
    UserDto User,
    TokenDto Token)
{
    [JsonConstructor]
    public AuthResultDto() : this(new UserDto(), new TokenDto()) { }
}
