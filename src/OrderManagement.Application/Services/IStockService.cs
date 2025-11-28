namespace OrderManagement.Application.Services;

/// <summary>
/// Serviço para gerenciamento de estoque
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Verifica disponibilidade de estoque para um produto com cor e tamanho específicos
    /// Retorna a filial que tem estoque disponível ou null se não houver
    /// </summary>
    Task<StockAvailabilityResult?> CheckAvailabilityAsync(int productId, int colorId, int sizeId, int quantity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reserva estoque para um pedido
    /// </summary>
    Task ReserveStockAsync(int skuId, int stockOfficeId, int quantity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Libera reserva de estoque
    /// </summary>
    Task ReleaseReservationAsync(int skuId, int stockOfficeId, int quantity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Decrementa estoque (baixa definitiva)
    /// </summary>
    Task DecreaseStockAsync(int skuId, int stockOfficeId, int quantity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado da verificação de disponibilidade de estoque
/// </summary>
public class StockAvailabilityResult
{
    public int SkuId { get; set; }
    public int StockOfficeId { get; set; }
    public int AvailableQuantity { get; set; }
    public string StockOfficeName { get; set; } = string.Empty;
}



