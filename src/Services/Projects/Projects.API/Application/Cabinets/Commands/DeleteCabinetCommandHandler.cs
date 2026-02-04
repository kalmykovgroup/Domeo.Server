using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Abstractions.Commands.Cabinets;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Application.Cabinets.Commands;

public sealed class DeleteCabinetCommandHandler : IRequestHandler<DeleteCabinetCommand>
{
    private readonly ProjectsDbContext _dbContext;

    public DeleteCabinetCommandHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteCabinetCommand request, CancellationToken cancellationToken)
    {
        var cabinet = await _dbContext.Cabinets.FindAsync([request.Id], cancellationToken);

        if (cabinet is null)
            throw new NotFoundException("Cabinet", request.Id);

        _dbContext.Cabinets.Remove(cabinet);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
