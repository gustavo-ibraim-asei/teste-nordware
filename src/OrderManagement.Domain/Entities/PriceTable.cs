namespace OrderManagement.Domain.Entities;

/// <summary>
/// Representa uma Tabela de Preços no sistema
/// </summary>
public class PriceTable : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<ProductPrice> ProductPrices { get; private set; } = new List<ProductPrice>();

    private PriceTable() { } // EF Core

    public PriceTable(string name, string? description, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome não pode ser vazio", nameof(name));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        if (name.Length > 200)
            throw new ArgumentException("O nome não pode exceder 200 caracteres", nameof(name));

        Name = name;
        Description = description;
        TenantId = tenantId;
        IsActive = true;
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

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

