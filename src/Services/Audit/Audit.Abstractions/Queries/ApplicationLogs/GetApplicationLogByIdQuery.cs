using Audit.Abstractions.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Audit.Abstractions.Queries.ApplicationLogs;

public sealed record GetApplicationLogByIdQuery(Guid Id) : IQuery<ApplicationLogDto>;
