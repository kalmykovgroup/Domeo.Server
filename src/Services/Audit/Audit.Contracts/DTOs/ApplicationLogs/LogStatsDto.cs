namespace Audit.Contracts.DTOs.ApplicationLogs;

public sealed record LogStatsDto(
    string? ServiceName,
    string? Level,
    int Count);

public sealed record LogStatsTotalDto(int Total);
