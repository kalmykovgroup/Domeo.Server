using System.Text.Json.Serialization;

namespace Domeo.Shared.Contracts.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTime CreatedAt)
{
    [JsonConstructor]
    public UserDto() : this(Guid.Empty, "", "", "", "", false, DateTime.MinValue) { }
}
