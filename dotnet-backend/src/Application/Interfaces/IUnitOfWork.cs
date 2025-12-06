namespace Application.Interfaces;

/// <summary>
/// Defines the contract for a unit of work that encapsulates multiple repository operations
/// within a single transactional scope. Provides methods for committing changes to the data store.
/// Used to coordinate the writing out of changes and to manage transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Asynchronously commits all changes made within the current unit of work to the underlying data store.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation. 
    /// The task result contains the number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangesAsync();
}
