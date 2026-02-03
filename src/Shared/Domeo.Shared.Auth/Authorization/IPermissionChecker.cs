namespace Domeo.Shared.Auth.Authorization;

/// <summary>
/// Interface for checking user permissions.
/// Implementations can use database, cache, or HTTP calls.
/// </summary>
public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
