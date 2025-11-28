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
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        if (name.Length > 200)
            throw new ArgumentException("Name must not exceed 200 characters", nameof(name));

        if (code.Length > 50)
            throw new ArgumentException("Code must not exceed 50 characters", nameof(code));

        Name = name;
        Code = code;
        Description = description;
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
    }

    public void UpdateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (code.Length > 50)
            throw new ArgumentException("Code must not exceed 50 characters", nameof(code));

        Code = code;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }
}

