using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Defines repository operations specific to <see cref="User"/> entities, 
/// in addition to generic CRUD methods.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Asynchronously retrieves a user by their unique username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// Returns the user if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Asynchronously retrieves a user by their unique email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// Returns the user if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Asynchronously retrieves a user associated with the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to search for.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// Returns the user if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}

/// <summary>
/// Defines repository operations specific to <see cref="Document"/> entities, 
/// in addition to generic CRUD methods.
/// </summary>
public interface IDocumentRepository : IGenericRepository<Document>
{
    /// <summary>
    /// Asynchronously retrieves all documents belonging to the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// Returns an enumerable collection of the user's documents.
    /// </returns>
    Task<IEnumerable<Document>> GetByUserIdAsync(int userId);
}

/// <summary>
/// Defines repository operations specific to <see cref="EventLog"/> entities, 
/// in addition to generic CRUD methods. This interface can be extended in the future 
/// to add domain-specific methods for event logs.
/// </summary>
public interface IEventLogRepository : IGenericRepository<EventLog>
{
    // Additional domain-specific methods for EventLog can be added here as needed.
}
