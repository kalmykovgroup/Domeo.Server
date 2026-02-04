using Audit.Abstractions.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Queries.ApplicationLogs;

public sealed record GetApplicationLogStatsQuery(
    string? GroupBy,
    DateTime? From,
    DateTime? To) : IQuery<object>;
