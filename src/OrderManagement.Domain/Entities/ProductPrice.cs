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
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        if (product.TenantId != tenantId)
            throw new ArgumentException("O produto deve pertencer ao mesmo tenant", nameof(product));

        if (priceTable.TenantId != tenantId)
            throw new ArgumentException("A tabela de preços deve pertencer ao mesmo tenant", nameof(priceTable));

        if (unitPrice < 0)
            throw new ArgumentException("O preço unitário não pode ser negativo", nameof(unitPrice));

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
            throw new ArgumentException("O preço unitário não pode ser negativo", nameof(unitPrice));

        UnitPrice = unitPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}

