using Amazon.S3;
using Amazon.S3.Model;
using Modules.Domain.Entities;

namespace Modules.Infrastructure.Storage;

public sealed class S3FileStorageService : IFileStorageService, IDisposable
{
    private const string GlbPrefix = "glb/";
    private readonly AmazonS3Client _client;
    private readonly string _bucket;
    private readonly string _endpoint;

    public S3FileStorageService(StorageConnection connection)
    {
        _bucket = connection.Bucket;
        _endpoint = connection.Endpoint;

        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{connection.Endpoint}",
            ForcePathStyle = true
        };

        _client = new AmazonS3Client(connection.AccessKey, connection.SecretKey, config);
    }

    public async Task<string> UploadAsync(string fileName, Stream stream, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = GlbPrefix + fileName,
            InputStream = stream,
            ContentType = "model/gltf-binary"
        };

        await _client.PutObjectAsync(request, ct);
        return GetPublicUrl(fileName);
    }

    public async Task DeleteAsync(string fileName, CancellationToken ct = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = GlbPrefix + fileName
        };

        await _client.DeleteObjectAsync(request, ct);
    }

    public Task<string> GetUrlAsync(string fileName, CancellationToken ct = default)
    {
        return Task.FromResult(GetPublicUrl(fileName));
    }

    private string GetPublicUrl(string fileName)
    {
        return $"https://{_bucket}.{_endpoint}/{GlbPrefix}{fileName}";
    }

    public void Dispose() => _client.Dispose();
}
