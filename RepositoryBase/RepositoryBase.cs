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

    /// <summary>
    /// Checks if the table contains any entities
    /// </summary>
    /// <returns>true - if table is empty, false - if table is not empty</returns>
    public async Task<bool> CheckEmptyTableAsync()
    {
        return !await _set.AnyAsync();
    }

    /// <summary>
    /// Counts how many entries match the filter
    /// </summary>
    /// <param name="filter">Filter for the count</param>
    /// <returns>The number of entities matching the filter</returns>
    public async Task<int> CountAsync(Expression<Func<T, bool>> filter = default)
    {
        return filter == default? await _set.CountAsync() : await _set.CountAsync(filter);
    }

    /// <summary>
    /// Finds an entity with the given primary key values. If an entity with the given
    ///     primary key values is being tracked by the context, then it is returned immediately
    ///     without making a request to the database. Otherwise, a query is made to the database
    ///     for an entity with the given primary key values and this entity, if found, is
    ///     attached to the context and returned. If no entity is found, then null is returned.
    /// </summary>
    /// <param name="ID">The values of the primary key for the entity to be found.</param>
    /// <returns>The entity found, or null.</returns>
    public async ValueTask<T> FindAsync(K ID)
    {
        return await _set.FindAsync(ID);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a specified condition.
    /// </summary>
    /// <param name="filter">Filter for the entity</param>
    /// <param name="noTracking">Defines if the entities will be tracked</param>
    /// <returns>The matching element</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public async Task<T> FirstAsync(Expression<Func<T, bool>> filter, bool noTracking = default)
    {
        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var query = ApplyNoTracking(noTracking);

        return await query.FirstAsync(filter);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a specified condition.
    /// </summary>
    /// <param name="filter">Filter for the entity</param>
    /// <param name="noTracking">Defines if the entities will be tracked</param>
    /// <returns>The matching element or it's defaul value</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, bool noTracking = default)
    {
        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var query = ApplyNoTracking(noTracking);

        return await query.FirstOrDefaultAsync(filter);
    }

    /// <summary>
    /// Gets all entities from DB matching the parameters
    /// </summary>
    /// <param name="filter">Filters the sequence</param>
    /// <param name="skip">Page number</param>
    /// <param name="take">Page size</param>
    /// <param name="noTracking">Defines if the entities will be tracked</param>
    /// <param name="orderBy">Order by query</param>
    /// <param name="include">Include query</param>
    /// <returns>The resulted list</returns>
    public async Task<IList<T>> GetAllAsync(
        Expression<Func<T, bool>> filter = default, 
        int skip = default, 
        int take = int.MaxValue, 
        bool noTracking = default, 
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = default, 
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = default)
    {
        var query = noTracking ? _set.AsNoTracking().AsQueryable() : _set.AsTracking().AsQueryable();
        
        if(filter != default)
        {
            query = query.Where(filter);
        }

        if (include != default)
        {
            query = include(query);
        }

        if (orderBy != default)
        {
            query = orderBy(query).AsQueryable();
        }

        query = skip == default ? query.Take(take) : query.Skip(skip).Take(take);
        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets all entities from DB matching the parameters with custom select
    /// </summary>
    /// <param name="filter">Filters the sequence</param>
    /// <param name="skip">Page number</param>
    /// <param name="take">Page size</param>
    /// <param name="noTracking">Defines if the entities will be tracked</param>
    /// <param name="orderBy">Order by query</param>
    /// <param name="include">Include query</param>
    /// <param name="selector">Custom selector</param>
    /// <returns>The resulted list</returns>
    public async Task<IList<object>> GetAllCustomSelectorAsync(
        Expression<Func<T, object>> selector,
        Expression<Func<T, bool>> filter = default,
        int skip = default,
        int take = int.MaxValue,
        bool noTracking = default,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = default,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = default)
    {
        if (selector == default)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        var query = ApplyNoTracking(noTracking);
        query = ApplyFilter(filter, query);
        query = ApplyInclude(include, query);
        query = ApplyOrderBy(orderBy, query);
        query = ApplyPagination(skip, take, query);

        return await query.Select(selector).ToListAsync();
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence that satisfies a specified
    ///     condition or a default value if no such element exists; this method throws an
    ///     exception if more than one element satisfies the condition..
    /// </summary>
    /// <param name="filter">Filter for the entity</param>
    /// <param name="noTracking">Defines if the entities will be tracked</param>
    /// <returns>The matching element or it's defaul value</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<T> SingleAsync(Expression<Func<T, bool>> filter, bool noTracking)
    {
        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var query = ApplyNoTracking(noTracking);

        return await query.SingleAsync(filter);
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence that satisfies a specified
    ///     condition or a default value if no such element exists; this method throws an
    ///     exception if more than one element satisfies the condition..
    /// </summary>
    /// <param name="filter">Filter for the entity</param>
    /// <param name="noTracking">Defines if the entities will be tracked</param>
    /// <returns>The matching element or it's defaul value</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> filter, bool noTracking = default)
    {
        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var query = ApplyNoTracking(noTracking);

        return await query.SingleOrDefaultAsync(filter);
    }

    #region Private helpers

    private IQueryable<T> ApplyNoTracking(bool noTracking)
    {
        return noTracking ? _set.AsNoTracking().AsQueryable() : _set.AsTracking().AsQueryable();
    }

    private static IQueryable<T> ApplyPagination(int skip, int take, IQueryable<T> query)
    {
        query = skip == default ? query.Take(take) : query.Skip(skip).Take(take);
        return query;
    }

    private static IQueryable<T> ApplyOrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, IQueryable<T> query)
    {
        if (orderBy != default)
        {
            query = orderBy(query).AsQueryable();
        }

        return query;
    }

    private static IQueryable<T> ApplyInclude(Func<IQueryable<T>, IIncludableQueryable<T, object>> include, IQueryable<T> query)
    {
        if (include != default)
        {
            query = include(query);
        }

        return query;
    }

    private static IQueryable<T> ApplyFilter(Expression<Func<T, bool>> filter, IQueryable<T> query)
    {
        if (filter != default)
        {
            query = query.Where(filter);
        }

        return query;
    }

    
    #endregion
}
