namespace OrderManagement.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string TenantId { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User() { } // EF Core

    public User(string email, string userName, string passwordHash, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio", nameof(email));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Nome de usuário não pode ser vazio", nameof(userName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Hash da senha não pode ser vazio", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        Email = email;
        UserName = userName;
        PasswordHash = passwordHash;
        TenantId = tenantId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Hash da senha não pode ser vazio", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }
}





