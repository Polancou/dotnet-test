using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Represents a repository for managing user entities.
/// </summary>
public class UserRepository(ApplicationDbContext context) : GenericRepository<User>(context), IUserRepository
{
    /// <summary>
    /// Asynchronously retrieves a user by their username, including their associated role.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, otherwise null.</returns>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// Asynchronously retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, otherwise null.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}

/// <summary>
/// Represents a repository for managing document entities.
/// </summary>
public class DocumentRepository(ApplicationDbContext context)
    : GenericRepository<Document>(context), IDocumentRepository
{
    /// <summary>
    /// Asynchronously retrieves all documents uploaded by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose documents are to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of documents.</returns>
    public async Task<IEnumerable<Document>> GetByUserIdAsync(int userId)
    {
        return await _context.Documents.Where(d => d.UploadedByUserId == userId).ToListAsync();
    }
}

/// <summary>
/// Represents a repository for managing event log entities.
/// </summary>
public class EventLogRepository : GenericRepository<EventLog>, IEventLogRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public EventLogRepository(ApplicationDbContext context) : base(context)
    {
    }
}
