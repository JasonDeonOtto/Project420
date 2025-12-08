using System.Linq.Expressions;

namespace Project420.Management.DAL.Repositories.Common;

/// <summary>
/// Generic repository interface for common CRUD operations.
/// All specific repositories inherit from this.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its primary key.
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all entities (active only, soft deletes excluded).
    /// </summary>
    /// <returns>Collection of all active entities</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    /// <param name="predicate">Filter expression</param>
    /// <returns>Matching entities</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <returns>Added entity with generated ID</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">Entity to update</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Performs a soft delete on an entity (sets IsDeleted = true).
    /// Required for POPIA compliance - data retention.
    /// </summary>
    /// <param name="id">Primary key of entity to delete</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if an entity exists by ID.
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of entities (active only).
    /// </summary>
    /// <returns>Count of active entities</returns>
    Task<int> CountAsync();

    /// <summary>
    /// Gets paginated results.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Page of entities</returns>
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
}
