namespace OrderManagement.Domain.Entities;

/// <summary>
/// Representa um Cliente no sistema
/// </summary>
public class Customer : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Document { get; private set; } // CPF/CNPJ
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();

    private Customer() { } // EF Core

    public Customer(string name, string email, string? phone, string? document, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        if (name.Length > 200)
            throw new ArgumentException("Name must not exceed 200 characters", nameof(name));

        if (email.Length > 200)
            throw new ArgumentException("Email must not exceed 200 characters", nameof(email));

        if (phone != null && phone.Length > 20)
            throw new ArgumentException("Phone must not exceed 20 characters", nameof(phone));

        if (document != null && document.Length > 20)
            throw new ArgumentException("Document must not exceed 20 characters", nameof(document));

        Name = name;
        Email = email;
        Phone = phone;
        Document = document;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Name must not exceed 200 characters", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (email.Length > 200)
            throw new ArgumentException("Email must not exceed 200 characters", nameof(email));

        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePhone(string? phone)
    {
        if (phone != null && phone.Length > 20)
            throw new ArgumentException("Phone must not exceed 20 characters", nameof(phone));

        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDocument(string? document)
    {
        if (document != null && document.Length > 20)
            throw new ArgumentException("Document must not exceed 20 characters", nameof(document));

        Document = document;
        UpdatedAt = DateTime.UtcNow;
    }
}

