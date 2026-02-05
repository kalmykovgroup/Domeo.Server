using MediatR;

namespace Audit.Application.Queries.ApplicationLogs;

public sealed record GetApplicationLogStatsQuery(
    string? GroupBy,
    DateTime? From,
    DateTime? To) : IRequest<object>;
