using Domain.Common;

namespace Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; }
    public ICollection<User> Users { get; private set; } = new List<User>();

    private Role() { } // EF Core

    public Role(string name) : base(true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Name = name;
    }
}
