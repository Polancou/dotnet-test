using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Represents the application's database context, providing access to the application's entities.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for <see cref="User"/> entities.
    /// </summary>
    public DbSet<User> Users { get; set; }



    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for <see cref="Document"/> entities.
    /// </summary>
    public DbSet<Document> Documents { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for <see cref="EventLog"/> entities.
    /// </summary>
    public DbSet<EventLog> EventLogs { get; set; }

    /// <summary>
    /// Configures the schema needed for the application database context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the relationship for EventLog associated with a User
        modelBuilder.Entity<EventLog>()
            .HasOne(e => e.User) // An EventLog is associated with one User
            .WithMany() // The User can have many EventLogs, but we don't need a navigation property on User
            .HasForeignKey(e => e.UserId); // The foreign key is UserId in the EventLog entity

        // Configure User Role conversion
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // Seed Admin User
        modelBuilder.Entity<User>().HasData(
            new { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = "Admin123!", Role = UserRole.Admin, IsActive = true, CreationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ModificationDate = (DateTime?)null }
        );
    }
}
