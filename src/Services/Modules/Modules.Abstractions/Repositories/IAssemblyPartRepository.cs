using Modules.Abstractions.Entities;

namespace Modules.Abstractions.Repositories;

public interface IAssemblyPartRepository : IRepository<AssemblyPart, Guid>
{
    Task<List<AssemblyPart>> GetByAssemblyIdAsync(Guid assemblyId, CancellationToken ct = default);
}
