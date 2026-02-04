namespace Domeo.Shared.Application;

/// <summary>
/// Unit of Work pattern abstraction
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
