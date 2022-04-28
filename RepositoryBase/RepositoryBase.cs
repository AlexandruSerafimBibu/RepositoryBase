using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace RepositoryBase;

public class RepositoryBase<T, C, K> : IRepositoryBase<T, K>
        where T : class
        where C : DbContext
{
    protected readonly DbSet<T> _set;
    protected readonly DbContext _dbContext;

    public RepositoryBase(C dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _set = dbContext.Set<T>();
    }

    public async ValueTask<T> FindAsync(K id)
    {
        return await _set.FindAsync(id);
    }

    public async Task<IList<T>> GetAllAsync(Expression<Func<T, bool>> filter, int skip, int take, bool noTracking, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
    {
        IQueryable<T> query = noTracking ? _set.AsNoTracking().AsQueryable() : _set.AsQueryable();
        
        if(filter != null)
        {
            query = query.Where(filter);
        }

        if (include != null)
        {
            query = include(query);
        }
        if (orderBy != null)
        {
            query = orderBy(query).AsQueryable();
        }
        query = skip == 0 ? query.Take(take) : query.Skip(skip).Take(take);

        return await query.ToListAsync();
    }


}
