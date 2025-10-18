using Microsoft.EntityFrameworkCore;
using ProductInventoryAPI.Infrastructure.Data;
using System.Linq.Expressions;

namespace ProductInventoryAPI.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation with common Entity Framework operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class BaseRepository<T> where T : class
{
    protected readonly ProductInventoryDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(ProductInventoryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Get entities with filter
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            return await orderBy(query).ToListAsync();
        }
        else
        {
            return await query.ToListAsync();
        }
    }

    /// <summary>
    /// Get paged entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetPagedAsync(
        int page, 
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    /// <summary>
    /// Count entities with optional filter
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.CountAsync();
    }

    /// <summary>
    /// Check if entity exists
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbSet.AnyAsync(filter);
    }

    /// <summary>
    /// Add entity
    /// </summary>
    public virtual async Task<T> CreateAsync(T entity)
    {
        _dbSet.Add(entity);
        return entity;
    }

    /// <summary>
    /// Update entity
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        _dbSet.Remove(entity);
        return true;
    }

    /// <summary>
    /// Delete entity
    /// </summary>
    public virtual async Task<bool> DeleteAsync(T entity)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);
        return true;
    }

    /// <summary>
    /// Search entities with text filter
    /// </summary>
    public virtual async Task<IEnumerable<T>> SearchAsync(
        Expression<Func<T, bool>> searchFilter,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "")
    {
        return await GetAsync(searchFilter, orderBy, includeProperties);
    }
}