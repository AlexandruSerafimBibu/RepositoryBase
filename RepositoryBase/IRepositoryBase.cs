using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace RepositoryBase;

public interface IRepositoryBase<TEntity, K>
    where TEntity : class
{
    ValueTask<TEntity> FindAsync(K id);
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, int skip, int take, bool noTracking, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> filter);
}