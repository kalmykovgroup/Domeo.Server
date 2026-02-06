using MediatR;
using Modules.Contracts.DTOs.Storage;

namespace Modules.Application.Commands.Storage;

public sealed record CreateStorageConnectionCommand(
    string Name,
    string Endpoint,
    string Bucket,
    string Region,
    string AccessKey,
    string SecretKey) : IRequest<StorageConnectionDto>;
