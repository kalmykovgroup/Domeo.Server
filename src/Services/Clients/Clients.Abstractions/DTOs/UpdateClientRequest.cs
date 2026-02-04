namespace Clients.Abstractions.DTOs;

public sealed record UpdateClientRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes);
