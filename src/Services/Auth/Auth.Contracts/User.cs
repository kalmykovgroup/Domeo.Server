using System.Text.Json.Serialization;

namespace Auth.Contracts;

public sealed class User
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("role")]
    public required string Role { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
