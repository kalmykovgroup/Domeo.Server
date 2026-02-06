namespace Modules.Contracts.DTOs.Storage;

public sealed record CreateStorageConnectionRequest(
    string Name,
    string Endpoint,
    string Bucket,
    string Region,
    string AccessKey,
    string SecretKey);
