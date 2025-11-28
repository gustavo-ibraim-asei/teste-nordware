namespace OrderManagement.Domain.Entities;

/// <summary>
/// Representa um tamanho de produto
/// </summary>
public class Size : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }

    // Navigation properties
    public virtual ICollection<Sku> Skus { get; private set; } = new List<Sku>();

    private Size() { } // EF Core

    public Size(string name, string? code, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        Name = name;
        Code = code;
        TenantId = tenantId;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
    }

    public void UpdateCode(string? code)
    {
        Code = code;
    }
}

