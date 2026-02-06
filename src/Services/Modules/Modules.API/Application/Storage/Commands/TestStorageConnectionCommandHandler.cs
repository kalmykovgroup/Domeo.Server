using MediatR;
using Modules.Application.Commands.Storage;
using Modules.Domain.Repositories;
using Modules.Infrastructure.Storage;

namespace Modules.API.Application.Storage.Commands;

public sealed class TestStorageConnectionCommandHandler : IRequestHandler<TestStorageConnectionCommand, bool>
{
    private readonly IStorageConnectionRepository _repository;

    public TestStorageConnectionCommandHandler(IStorageConnectionRepository repository)
        => _repository = repository;

    public async Task<bool> Handle(
        TestStorageConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"StorageConnection {request.Id} not found");

        try
        {
            using var s3 = new S3FileStorageService(connection);
            using var testStream = new MemoryStream("test"u8.ToArray());
            await s3.UploadAsync("_connection_test.tmp", testStream, cancellationToken);
            await s3.DeleteAsync("_connection_test.tmp", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
