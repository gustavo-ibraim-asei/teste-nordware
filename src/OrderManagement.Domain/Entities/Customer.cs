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
            throw new ArgumentException("O nome não pode ser vazio", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O email não pode ser vazio", nameof(email));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        if (name.Length > 200)
            throw new ArgumentException("O nome não pode exceder 200 caracteres", nameof(name));

        if (email.Length > 200)
            throw new ArgumentException("O email não pode exceder 200 caracteres", nameof(email));

        if (phone != null && phone.Length > 20)
            throw new ArgumentException("O telefone não pode exceder 20 caracteres", nameof(phone));

        if (document != null && document.Length > 20)
            throw new ArgumentException("O documento não pode exceder 20 caracteres", nameof(document));

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
            throw new ArgumentException("O nome não pode ser vazio", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("O nome não pode exceder 200 caracteres", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O email não pode ser vazio", nameof(email));

        if (email.Length > 200)
            throw new ArgumentException("O email não pode exceder 200 caracteres", nameof(email));

        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePhone(string? phone)
    {
        if (phone != null && phone.Length > 20)
            throw new ArgumentException("O telefone não pode exceder 20 caracteres", nameof(phone));

        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDocument(string? document)
    {
        if (document != null && document.Length > 20)
            throw new ArgumentException("O documento não pode exceder 20 caracteres", nameof(document));

        Document = document;
        UpdatedAt = DateTime.UtcNow;
    }
}

