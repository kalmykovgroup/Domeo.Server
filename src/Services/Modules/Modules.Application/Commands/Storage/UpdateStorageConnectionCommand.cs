using MediatR;
using Modules.Contracts.DTOs.Storage;

namespace Modules.Application.Commands.Storage;

public sealed record UpdateStorageConnectionCommand(
    Guid Id,
    string Name,
    string Endpoint,
    string Bucket,
    string Region,
    string AccessKey,
    string SecretKey,
    bool IsActive) : IRequest<StorageConnectionDto>;
