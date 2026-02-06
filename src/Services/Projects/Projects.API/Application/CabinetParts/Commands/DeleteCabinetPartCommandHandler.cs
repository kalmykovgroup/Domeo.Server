using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Application.Commands.CabinetParts;
using Projects.Infrastructure.Persistence;

namespace Projects.API.Application.CabinetParts.Commands;

public sealed class DeleteCabinetPartCommandHandler : IRequestHandler<DeleteCabinetPartCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public DeleteCabinetPartCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteCabinetPartCommand request, CancellationToken cancellationToken)
    {
        var part = await _dbContext.CabinetParts.FindAsync([request.Id], cancellationToken);

        if (part is null)
            throw new NotFoundException("CabinetPart", request.Id);

        _dbContext.CabinetParts.Remove(part);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
