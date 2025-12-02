namespace Domain.Common;

/// <summary>
/// Represents the base class for all entities in the domain.
/// Provides common properties like Id, active status, and timestamps for creation and modification.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the entity is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when the entity was created.
    /// </summary>
    public DateTime CreationDate { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when the entity was last modified. Null if never modified.
    /// </summary>
    public DateTime? ModificationDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class.
    /// This constructor is primarily for ORM (e.g., EF Core) materialization.
    /// </summary>
    protected BaseEntity()
    {
        // Required by EF Core for object materialization
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class with an initial active status.
    /// Sets the creation date to the current UTC time.
    /// </summary>
    /// <param name="isActive">A boolean indicating whether the entity should be active upon creation. Defaults to true.</param>
    protected BaseEntity(bool isActive = true)
    {
        IsActive = isActive;
        CreationDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the <see cref="ModificationDate"/> property to the current UTC time.
    /// </summary>
    public void UpdateModificationDate()
    {
        ModificationDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the entity by setting <see cref="IsActive"/> to false and updates the modification date.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdateModificationDate();
    }

    /// <summary>
    /// Activates the entity by setting <see cref="IsActive"/> to true and updates the modification date.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdateModificationDate();
    }
}
