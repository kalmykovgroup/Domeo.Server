using Microsoft.AspNetCore.Authorization;

namespace Auth.Contracts.Authorization;

public sealed class InternalApiKeyRequirement : IAuthorizationRequirement
{
}
