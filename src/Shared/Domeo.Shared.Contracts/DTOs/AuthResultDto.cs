namespace Domeo.Shared.Contracts.DTOs;

public sealed record AuthResultDto(
    UserDto User,
    TokenDto Token);
