using Modules.Abstractions.Entities;

namespace Modules.Abstractions.Repositories;

public interface IHardwareRepository : IRepository<Hardware, int>
{
    Task<List<Hardware>> GetHardwareAsync(string? type, bool? activeOnly, CancellationToken ct = default);
}
