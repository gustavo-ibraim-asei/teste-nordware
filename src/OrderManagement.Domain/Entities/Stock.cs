namespace OrderManagement.Domain.Entities;

/// <summary>
/// Representa o estoque de um SKU em uma filial específica
/// </summary>
public class Stock : BaseEntity
{
    public int SkuId { get; private set; }
    public int StockOfficeId { get; private set; }
    public int Quantity { get; private set; }
    public int Reserved { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public virtual Sku Sku { get; private set; } = null!;
    public virtual StockOffice StockOffice { get; private set; } = null!;

    private Stock() { } // EF Core

    public Stock(int skuId, int stockOfficeId, int quantity, string tenantId)
    {
        if (skuId <= 0)
            throw new ArgumentException("O ID do SKU deve ser maior que zero", nameof(skuId));

        if (stockOfficeId <= 0)
            throw new ArgumentException("O ID da filial deve ser maior que zero", nameof(stockOfficeId));

        if (quantity < 0)
            throw new ArgumentException("A quantidade não pode ser negativa", nameof(quantity));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        SkuId = skuId;
        StockOfficeId = stockOfficeId;
        Quantity = quantity;
        Reserved = 0;
        TenantId = tenantId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retorna a quantidade disponível (Quantity - Reserved)
    /// </summary>
    public int AvailableQuantity => Quantity - Reserved;

    /// <summary>
    /// Reserva uma quantidade do estoque
    /// </summary>
    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero", nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException($"Estoque insuficiente. Disponível: {AvailableQuantity}, Solicitado: {quantity}");

        Reserved += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Libera uma quantidade reservada
    /// </summary>
    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero", nameof(quantity));

        if (Reserved < quantity)
            throw new InvalidOperationException($"Não é possível liberar mais do que foi reservado. Reservado: {Reserved}, Solicitado: {quantity}");

        Reserved -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrementa a quantidade do estoque (baixa definitiva)
    /// </summary>
    public void Decrease(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero", nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException($"Estoque insuficiente. Disponível: {AvailableQuantity}, Solicitado: {quantity}");

        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Incrementa a quantidade do estoque
    /// </summary>
    public void Increase(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero", nameof(quantity));

        Quantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza a quantidade total do estoque
    /// </summary>
    public void UpdateQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("A quantidade não pode ser negativa", nameof(quantity));

        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}

