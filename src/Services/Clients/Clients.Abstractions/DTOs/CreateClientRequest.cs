namespace Clients.Abstractions.DTOs;

public sealed record CreateClientRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes);
