using Application.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

/// <summary>
/// Implements the Unit of Work pattern, encapsulating all repository operations and managing database transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the underlying database.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes the underlying database context.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
    }
}
