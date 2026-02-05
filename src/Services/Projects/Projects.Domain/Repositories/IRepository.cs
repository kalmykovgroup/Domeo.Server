namespace Projects.Domain.Repositories;

public interface IRepository<T, in TId> where T : class
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}
