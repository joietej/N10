using System.Linq.Expressions;
using N10.Data.Entities;

namespace N10.Data.Repositories;

public interface IRepositoryBase<T> where T : class, IEntity
{
    Task<int> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    IQueryable<T> Query(string? property = null);
}
