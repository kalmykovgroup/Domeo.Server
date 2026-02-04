using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Domeo.Shared.Infrastructure.Redis;

/// <summary>
/// Stores events to file when Redis is unavailable.
/// Events are stored as JSON lines for easy reading and replay.
/// </summary>
public sealed class FallbackEventStore
{
    private readonly string _fallbackDirectory;
    private readonly ILogger<FallbackEventStore> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public FallbackEventStore(ILogger<FallbackEventStore> logger, string? fallbackDirectory = null)
    {
        _logger = logger;
        _fallbackDirectory = fallbackDirectory ?? Path.Combine(AppContext.BaseDirectory, "logs", "fallback");

        if (!Directory.Exists(_fallbackDirectory))
        {
            Directory.CreateDirectory(_fallbackDirectory);
        }
    }

    public string FallbackDirectory => _fallbackDirectory;

    public async Task StoreEventAsync(string channel, string message, CancellationToken cancellationToken = default)
    {
        var entry = new FallbackEventEntry
        {
            Channel = channel,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        var fileName = $"events_{DateTime.UtcNow:yyyyMMdd}.jsonl";
        var filePath = Path.Combine(_fallbackDirectory, fileName);

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var line = JsonSerializer.Serialize(entry) + Environment.NewLine;
            await File.AppendAllTextAsync(filePath, line, cancellationToken);
            _logger.LogDebug("Stored fallback event to {FilePath}, channel: {Channel}", filePath, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store fallback event to file");
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public IEnumerable<string> GetPendingFiles()
    {
        if (!Directory.Exists(_fallbackDirectory))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(_fallbackDirectory, "events_*.jsonl")
            .OrderBy(f => f);
    }

    public async IAsyncEnumerable<FallbackEventEntry> ReadEventsAsync(string filePath)
    {
        if (!File.Exists(filePath))
            yield break;

        using var reader = new StreamReader(filePath);
        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            FallbackEventEntry? entry = null;
            try
            {
                entry = JsonSerializer.Deserialize<FallbackEventEntry>(line);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize fallback event line");
            }

            if (entry is not null)
                yield return entry;
        }
    }

    public void MarkFileAsProcessed(string filePath)
    {
        try
        {
            var processedPath = filePath + ".processed";
            if (File.Exists(filePath))
            {
                File.Move(filePath, processedPath, overwrite: true);
                _logger.LogInformation("Marked fallback file as processed: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark file as processed: {FilePath}", filePath);
        }
    }

    public void CleanupOldFiles(int daysToKeep = 7)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var processedFiles = Directory.GetFiles(_fallbackDirectory, "*.processed");

            foreach (var file in processedFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffDate)
                {
                    File.Delete(file);
                    _logger.LogDebug("Deleted old processed fallback file: {FilePath}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup old fallback files");
        }
    }
}

public sealed record FallbackEventEntry
{
    public required string Channel { get; init; }
    public required string Message { get; init; }
    public required DateTime Timestamp { get; init; }
}
