using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Project420.Shared.Core.Entities;

namespace Project420.Inventory.DAL.Repositories.Common;

/// <summary>
/// Generic repository base class implementing common CRUD operations.
/// All specific repositories inherit from this.
/// </summary>
/// <typeparam name="T">Entity type that inherits from AuditableEntity</typeparam>
public class Repository<T> : IRepository<T> where T : AuditableEntity
{
    protected readonly InventoryDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(InventoryDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc/>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        // Query filter automatically excludes soft-deleted items
        return await _dbSet.ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            // Soft delete - POPIA compliance requires data retention
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            // DeletedBy will be set by DbContext.SaveChangesAsync override
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync()
    {
        // Query filter automatically excludes soft-deleted items
        return await _dbSet.CountAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
