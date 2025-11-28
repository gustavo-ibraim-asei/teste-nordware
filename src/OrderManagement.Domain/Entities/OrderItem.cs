namespace OrderManagement.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal { get; private set; }

    // Navigation property
    public virtual Order Order { get; private set; } = null!;

    private OrderItem() { } // EF Core

    public OrderItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId <= 0)
            throw new ArgumentException("Product ID must be greater than zero", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Subtotal = quantity * unitPrice;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        Quantity = newQuantity;
        Subtotal = Quantity * UnitPrice;
    }
}


