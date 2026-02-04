using Auth.Contracts;

namespace Auth.Contracts.DTOs;

public sealed record AuthResultDto(User User, TokenDto Token);
