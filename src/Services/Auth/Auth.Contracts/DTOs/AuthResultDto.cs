namespace Auth.Contracts.DTOs;

public sealed record AuthResultDto(UserDto User, TokenDto Token);
