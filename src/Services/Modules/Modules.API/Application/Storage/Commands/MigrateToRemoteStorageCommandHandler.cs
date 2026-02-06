using Domeo.Shared.Application;
using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Application.Commands.Storage;
using Modules.Domain.Entities.Shared;
using Modules.Domain.Repositories;
using Modules.Infrastructure.Storage;

namespace Modules.API.Application.Storage.Commands;

public sealed class MigrateToRemoteStorageCommandHandler : IRequestHandler<MigrateToRemoteStorageCommand, int>
{
    private readonly FileStorageResolver _resolver;
    private readonly IComponentRepository _componentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MigrateToRemoteStorageCommandHandler> _logger;

    public MigrateToRemoteStorageCommandHandler(
        FileStorageResolver resolver,
        IComponentRepository componentRepository,
        IUnitOfWork unitOfWork,
        ILogger<MigrateToRemoteStorageCommandHandler> logger)
    {
        _resolver = resolver;
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(
        MigrateToRemoteStorageCommand request, CancellationToken cancellationToken)
    {
        var s3 = await _resolver.GetS3StorageAsync(cancellationToken)
            ?? throw new InvalidOperationException("No active S3 storage connection found");

        using (s3)
        {
            var localStorage = _resolver.GetLocalStorage();
            var localFiles = localStorage.GetLocalFiles();

            if (localFiles.Length == 0)
                return 0;

            var migrated = 0;

            foreach (var filePath in localFiles)
            {
                var fileName = Path.GetFileName(filePath);

                try
                {
                    await using var stream = File.OpenRead(filePath);
                    var newUrl = await s3.UploadAsync(fileName, stream, cancellationToken);

                    // Update component GlbUrl if this file belongs to a component
                    var components = await _componentRepository.GetAllAsync(cancellationToken);
                    foreach (var component in components)
                    {
                        if (component.Params is GlbParams glb && glb.GlbUrl.Contains(fileName))
                        {
                            component.Update(component.Name, new GlbParams(newUrl, glb.Scale), component.Tags);
                            _componentRepository.Update(component);
                        }
                    }

                    File.Delete(filePath);
                    migrated++;
                    _logger.LogInformation("Migrated {FileName} to S3", fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to migrate {FileName}", fileName);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return migrated;
        }
    }
}
