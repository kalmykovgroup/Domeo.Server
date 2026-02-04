using Audit.Abstractions.DTOs;
using MediatR;

namespace Audit.Abstractions.Queries.ApplicationLogs;

public sealed record GetApplicationLogByIdQuery(Guid Id) : IRequest<ApplicationLogDto>;
