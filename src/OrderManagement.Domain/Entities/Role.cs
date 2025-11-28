namespace OrderManagement.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { } // EF Core

    public Role(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da role não pode ser vazio", nameof(name));

        Name = name;
        Description = description;
    }
}


