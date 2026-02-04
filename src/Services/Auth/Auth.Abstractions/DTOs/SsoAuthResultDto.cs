using Domeo.Shared.Contracts.DTOs;

namespace Auth.Abstractions.DTOs;

public sealed record SsoAuthResultDto(
    SsoUserDto User,
    TokenDto Token);
