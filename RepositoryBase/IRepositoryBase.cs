using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace RepositoryBase;

public interface IRepositoryBase<TEntity, K>
    where TEntity : class
{
    ValueTask<TEntity> FindAsync(K id);
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> filter, bool noTracking);
    Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> filter, bool noTracking);
    Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, bool noTracking);
    Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> filter, bool noTracking);
    Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, int skip, int take, bool noTracking, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> filter);
    Task<bool> CheckEmptyTableAsync();
    Task<IList<object>> GetAllCustomSelectorAsync(Expression<Func<TEntity, object>> selector, Expression<Func<TEntity, bool>> filter, int skip, int take, bool noTracking, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include);
}