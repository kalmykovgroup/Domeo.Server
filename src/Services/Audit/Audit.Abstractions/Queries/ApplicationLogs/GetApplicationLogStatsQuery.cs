using MediatR;

namespace Audit.Abstractions.Queries.ApplicationLogs;

public sealed record GetApplicationLogStatsQuery(
    string? GroupBy,
    DateTime? From,
    DateTime? To) : IRequest<object>;
