using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Modules.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private const string UploadPath = "uploads/glb";
    private readonly string _rootPath;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _rootPath = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), UploadPath);
        _httpContextAccessor = httpContextAccessor;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> UploadAsync(string fileName, Stream stream, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_rootPath, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream, ct);
        return GetPublicUrl(fileName);
    }

    public Task DeleteAsync(string fileName, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_rootPath, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string fileName, CancellationToken ct = default)
    {
        return Task.FromResult(GetPublicUrl(fileName));
    }

    public string[] GetLocalFiles()
    {
        if (!Directory.Exists(_rootPath))
            return [];
        return Directory.GetFiles(_rootPath);
    }

    private string GetPublicUrl(string fileName)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request is not null
            ? $"{request.Scheme}://{request.Host}"
            : "";
        return $"{baseUrl}/{UploadPath}/{fileName}";
    }
}
