using Modules.Domain.Entities;

namespace Modules.Domain.Repositories;

public interface IAssemblyPartRepository : IRepository<AssemblyPart, Guid>
{
    Task<List<AssemblyPart>> GetByAssemblyIdAsync(Guid assemblyId, CancellationToken ct = default);
}
