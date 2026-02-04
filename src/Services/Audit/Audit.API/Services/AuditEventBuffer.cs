using System.Text.Json;
using System.Threading.Channels;
using Domeo.Shared.Events;

namespace Audit.API.Services;

public interface IAuditEventBuffer
{
    ValueTask EnqueueAuditAsync(AuditEvent auditEvent, CancellationToken ct = default);
    ValueTask EnqueueSessionAsync(object sessionEvent, string eventType, CancellationToken ct = default);
    IAsyncEnumerable<BufferedEvent> DequeueAllAsync(CancellationToken ct = default);
    int Count { get; }
}

public sealed record BufferedEvent(string EventType, string Payload, DateTimeOffset Timestamp);

public sealed class AuditEventBuffer : IAuditEventBuffer, IDisposable
{
    private readonly Channel<BufferedEvent> _channel;
    private readonly string _bufferFilePath;
    private readonly string _processingFilePath;
    private readonly ILogger<AuditEventBuffer> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditEventBuffer(ILogger<AuditEventBuffer> logger, IConfiguration configuration)
    {
        _logger = logger;

        // Use bounded channel to prevent memory issues
        _channel = Channel.CreateBounded<BufferedEvent>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        // Buffer files in app data directory
        var bufferDir = configuration["Audit:BufferDirectory"]
            ?? Path.Combine(AppContext.BaseDirectory, "audit-buffer");
        Directory.CreateDirectory(bufferDir);

        _bufferFilePath = Path.Combine(bufferDir, "audit-buffer.jsonl");
        _processingFilePath = Path.Combine(bufferDir, "audit-buffer-processing.jsonl");

        // Load any pending events from previous run
        _ = LoadPendingEventsAsync();
    }

    public int Count => _channel.Reader.Count;

    public async ValueTask EnqueueAuditAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        var bufferedEvent = new BufferedEvent(
            "AuditEvent",
            JsonSerializer.Serialize(auditEvent, JsonOptions),
            DateTimeOffset.UtcNow);

        await EnqueueAndPersistAsync(bufferedEvent, ct);
    }

    public async ValueTask EnqueueSessionAsync(object sessionEvent, string eventType, CancellationToken ct = default)
    {
        var bufferedEvent = new BufferedEvent(
            eventType,
            JsonSerializer.Serialize(sessionEvent, sessionEvent.GetType(), JsonOptions),
            DateTimeOffset.UtcNow);

        await EnqueueAndPersistAsync(bufferedEvent, ct);
    }

    public async IAsyncEnumerable<BufferedEvent> DequeueAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        while (_channel.Reader.TryRead(out var item))
        {
            yield return item;
        }

        // Clear the buffer file after successful dequeue
        await ClearBufferFileAsync();
    }

    private async ValueTask EnqueueAndPersistAsync(BufferedEvent bufferedEvent, CancellationToken ct)
    {
        // Write to channel
        await _channel.Writer.WriteAsync(bufferedEvent, ct);

        // Persist to file for crash recovery
        await PersistToFileAsync(bufferedEvent);

        _logger.LogDebug("Buffered event: {EventType}", bufferedEvent.EventType);
    }

    private async Task PersistToFileAsync(BufferedEvent bufferedEvent)
    {
        await _fileLock.WaitAsync();
        try
        {
            var line = JsonSerializer.Serialize(bufferedEvent, JsonOptions);
            await File.AppendAllTextAsync(_bufferFilePath, line + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist buffered event to file");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task ClearBufferFileAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            if (File.Exists(_bufferFilePath))
            {
                File.Delete(_bufferFilePath);
            }
            if (File.Exists(_processingFilePath))
            {
                File.Delete(_processingFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear buffer file");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task LoadPendingEventsAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            // First, check for processing file (interrupted flush)
            if (File.Exists(_processingFilePath))
            {
                await LoadFromFileAsync(_processingFilePath);
                File.Delete(_processingFilePath);
            }

            // Then check for regular buffer file
            if (File.Exists(_bufferFilePath))
            {
                await LoadFromFileAsync(_bufferFilePath);
                File.Delete(_bufferFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load pending events from buffer file");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task LoadFromFileAsync(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        var loadedCount = 0;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var bufferedEvent = JsonSerializer.Deserialize<BufferedEvent>(line, JsonOptions);
                if (bufferedEvent is not null)
                {
                    await _channel.Writer.WriteAsync(bufferedEvent);
                    loadedCount++;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize buffered event: {Line}", line);
            }
        }

        if (loadedCount > 0)
        {
            _logger.LogInformation("Loaded {Count} pending events from buffer file", loadedCount);
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
        _fileLock.Dispose();
    }
}
