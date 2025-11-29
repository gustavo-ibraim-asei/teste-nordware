namespace OrderManagement.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public int? SkuId { get; private set; }
    public int? StockOfficeId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal { get; private set; }

    // Navigation properties
    public virtual Order Order { get; private set; } = null!;
    public virtual Sku? Sku { get; private set; }
    public virtual StockOffice? StockOffice { get; private set; }

    private OrderItem() { } // EF Core

    public OrderItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId <= 0)
            throw new ArgumentException("O ID do produto deve ser maior que zero", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("O nome do produto não pode ser vazio", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("O preço unitário não pode ser negativo", nameof(unitPrice));

        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Subtotal = quantity * unitPrice;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero", nameof(newQuantity));

        Quantity = newQuantity;
        Subtotal = Quantity * UnitPrice;
    }

    public void SetStockInfo(int skuId, int stockOfficeId)
    {
        if (skuId <= 0)
            throw new ArgumentException("O ID do SKU deve ser maior que zero", nameof(skuId));

        if (stockOfficeId <= 0)
            throw new ArgumentException("O ID da filial deve ser maior que zero", nameof(stockOfficeId));

        SkuId = skuId;
        StockOfficeId = stockOfficeId;
    }
}


