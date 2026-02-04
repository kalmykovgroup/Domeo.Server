namespace Auth.Contracts;

public sealed class User
{
    public required Guid Id { get; init; }
    public required string Role { get; init; }
    public string? Email { get; init; }
    public string? Name { get; init; }
}
