using System;

namespace Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreationDate { get; private set; }
    public DateTime? ModificationDate { get; private set; }

    protected BaseEntity()
    {
        // Required by EF Core
    }

    protected BaseEntity(bool isActive = true)
    {
        IsActive = isActive;
        CreationDate = DateTime.UtcNow;
    }

    public void UpdateModificationDate()
    {
        ModificationDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateModificationDate();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateModificationDate();
    }
}
