using System.Linq.Expressions;
using Application.Interfaces;
using Domain.Common;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implements a generic repository for entities inheriting from <see cref="BaseEntity"/>.
/// Provides common CRUD operations.
/// </summary>
/// <typeparam name="T">The type of entity this repository handles, must inherit from <see cref="BaseEntity"/>.</typeparam>
public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T>
    where T : BaseEntity
{
    /// <summary>
    /// The database context used by the repository.
    /// </summary>
    protected readonly ApplicationDbContext _context = context;

    /// <summary>
    /// Retrieves an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the entity if found, otherwise null.</returns>
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    /// <summary>
    /// Retrieves all entities of the generic type asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning an enumerable collection of all entities.</returns>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    /// <summary>
    /// Finds entities based on a provided predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A LINQ expression to filter entities.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning an enumerable collection of entities that match the predicate.</returns>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Adds a new entity to the database asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    /// <summary>
    /// Removes an entity from the database.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}
