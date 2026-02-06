namespace Modules.Contracts.DTOs.Storage;

public sealed record UpdateStorageConnectionRequest(
    string Name,
    string Endpoint,
    string Bucket,
    string Region,
    string AccessKey,
    string SecretKey,
    bool IsActive);
