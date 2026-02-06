using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Modules.Domain.Repositories;

namespace Modules.Infrastructure.Storage;

public sealed class FileStorageResolver
{
    private readonly IStorageConnectionRepository _storageRepository;
    private readonly IServiceProvider _serviceProvider;

    public FileStorageResolver(
        IStorageConnectionRepository storageRepository,
        IServiceProvider serviceProvider)
    {
        _storageRepository = storageRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task<IFileStorageService> ResolveAsync(CancellationToken ct = default)
    {
        var active = await _storageRepository.GetActiveAsync(ct);
        if (active is not null)
            return new S3FileStorageService(active);

        return new LocalFileStorageService(
            _serviceProvider.GetRequiredService<IWebHostEnvironment>(),
            _serviceProvider.GetRequiredService<IHttpContextAccessor>());
    }

    public LocalFileStorageService GetLocalStorage()
    {
        return new LocalFileStorageService(
            _serviceProvider.GetRequiredService<IWebHostEnvironment>(),
            _serviceProvider.GetRequiredService<IHttpContextAccessor>());
    }

    public async Task<S3FileStorageService?> GetS3StorageAsync(CancellationToken ct = default)
    {
        var active = await _storageRepository.GetActiveAsync(ct);
        return active is not null ? new S3FileStorageService(active) : null;
    }
}
