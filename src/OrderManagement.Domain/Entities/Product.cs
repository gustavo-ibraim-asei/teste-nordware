namespace OrderManagement.Domain.Entities;

/// <summary>
/// Representa um Produto no sistema
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<Sku> Skus { get; private set; } = new List<Sku>();
    public virtual ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
    public virtual ICollection<ProductPrice> ProductPrices { get; private set; } = new List<ProductPrice>();

    private Product() { } // EF Core

    public Product(string name, string code, string? description, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome não pode ser vazio", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("O código não pode ser vazio", nameof(code));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        if (name.Length > 200)
            throw new ArgumentException("O nome não pode exceder 200 caracteres", nameof(name));

        if (code.Length > 50)
            throw new ArgumentException("O código não pode exceder 50 caracteres", nameof(code));

        Name = name;
        Code = code;
        Description = description;
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
    }

    public void UpdateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("O código não pode ser vazio", nameof(code));

        if (code.Length > 50)
            throw new ArgumentException("O código não pode exceder 50 caracteres", nameof(code));

        Code = code;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }
}

