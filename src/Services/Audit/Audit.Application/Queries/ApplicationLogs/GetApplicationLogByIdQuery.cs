using Audit.Contracts.DTOs.ApplicationLogs;
using MediatR;

namespace Audit.Application.Queries.ApplicationLogs;

public sealed record GetApplicationLogByIdQuery(Guid Id) : IRequest<ApplicationLogDto>;
