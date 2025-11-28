namespace OrderManagement.Domain.Entities;

public class UserRole : BaseEntity
{
    public int UserId { get; private set; }
    public int RoleId { get; private set; }

    // Navigation properties
    public virtual User User { get; private set; } = null!;
    public virtual Role Role { get; private set; } = null!;

    private UserRole() { } // EF Core

    public UserRole(int userId, int roleId)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID deve ser maior que zero", nameof(userId));

        if (roleId <= 0)
            throw new ArgumentException("Role ID deve ser maior que zero", nameof(roleId));

        UserId = userId;
        RoleId = roleId;
    }
}


