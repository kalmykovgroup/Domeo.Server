namespace Domeo.Shared.Kernel.Application.Abstractions;

/// <summary>
/// Unit of Work pattern abstraction
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
