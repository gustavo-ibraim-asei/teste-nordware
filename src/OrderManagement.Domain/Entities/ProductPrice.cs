namespace OrderManagement.Domain.Entities;

/// <summary>
/// Representa o preço de um produto em uma tabela de preços específica
/// </summary>
public class ProductPrice : BaseEntity
{
    public int ProductId { get; private set; }
    public int PriceTableId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public virtual Product Product { get; private set; } = null!;
    public virtual PriceTable PriceTable { get; private set; } = null!;

    private ProductPrice() { } // EF Core

    public ProductPrice(Product product, PriceTable priceTable, decimal unitPrice, string tenantId)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (priceTable == null)
            throw new ArgumentNullException(nameof(priceTable));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        if (product.TenantId != tenantId)
            throw new ArgumentException("Product must belong to the same tenant", nameof(product));

        if (priceTable.TenantId != tenantId)
            throw new ArgumentException("PriceTable must belong to the same tenant", nameof(priceTable));

        if (unitPrice < 0)
            throw new ArgumentException("UnitPrice cannot be negative", nameof(unitPrice));

        ProductId = product.Id;
        PriceTableId = priceTable.Id;
        Product = product;
        PriceTable = priceTable;
        UnitPrice = unitPrice;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentException("UnitPrice cannot be negative", nameof(unitPrice));

        UnitPrice = unitPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}

