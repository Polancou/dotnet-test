using System.Linq.Expressions;
using Domain.Common;

namespace Application.Interfaces;

/// <summary>
/// Defines a generic repository interface for performing common data access operations 
/// (CRUD) on entities deriving from <see cref="BaseEntity"/>.
/// </summary>
/// <typeparam name="T">
/// The type of entity handled by the repository. Must inherit from <see cref="BaseEntity"/>.
/// </typeparam>
public interface IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. 
    /// Returns the entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Asynchronously retrieves all entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. 
    /// Returns an enumerable collection of all entities.
    /// </returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Asynchronously finds entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">
    /// A LINQ expression used to filter entities (e.g., <c>x => x.IsActive</c>).
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. 
    /// Returns an enumerable collection of matching entities.
    /// </returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Asynchronously adds a new entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task AddAsync(T entity);

    /// <summary>
    /// Synchronously updates an existing entity in the data store.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    void Update(T entity);

    /// <summary>
    /// Synchronously removes an entity from the data store.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(T entity);
}
