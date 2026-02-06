using MediatR;
using Modules.Contracts.DTOs.Components;

namespace Modules.Application.Commands.Components;

public sealed record UploadComponentGlbCommand(
    Guid ComponentId,
    string FileName,
    Stream FileStream) : IRequest<ComponentDto>;
