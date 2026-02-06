namespace Modules.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<string> UploadAsync(string fileName, Stream stream, CancellationToken ct = default);
    Task DeleteAsync(string fileName, CancellationToken ct = default);
    Task<string> GetUrlAsync(string fileName, CancellationToken ct = default);
}
