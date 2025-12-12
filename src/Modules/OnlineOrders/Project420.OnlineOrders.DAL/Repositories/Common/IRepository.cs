using System.Linq.Expressions;
using Project420.Shared.Core.Entities;

namespace Project420.OnlineOrders.DAL.Repositories.Common;

/// <summary>
/// Generic repository interface defining common CRUD operations.
/// All specific repository interfaces inherit from this.
/// </summary>
/// <typeparam name="T">Entity type that inherits from AuditableEntity</typeparam>
public interface IRepository<T> where T : AuditableEntity
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Get all entities (excludes soft-deleted)
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Find entities matching predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Add new entity
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Update existing entity
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Soft delete entity (POPIA compliance - 7-year retention)
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Check if entity exists
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Count entities (excludes soft-deleted)
    /// </summary>
    Task<int> CountAsync();
}
