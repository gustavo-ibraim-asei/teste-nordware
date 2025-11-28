using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Services;

public class StockService : IStockService
{
    private readonly ISkuRepository _skuRepository;
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<StockService> _logger;

    public StockService(
        ISkuRepository skuRepository,
        IStockRepository stockRepository,
        ILogger<StockService> logger)
    {
        _skuRepository = skuRepository;
        _stockRepository = stockRepository;
        _logger = logger;
    }

    public async Task<StockAvailabilityResult?> CheckAvailabilityAsync(
        int productId,
        int colorId,
        int sizeId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Verificando disponibilidade de estoque: ProductId={ProductId}, ColorId={ColorId}, SizeId={SizeId}, Quantidade={Quantity}",
            productId, colorId, sizeId, quantity);

        // Buscar SKU
        Domain.Entities.Sku? sku = await _skuRepository.GetByProductColorSizeAsync(productId, colorId, sizeId, cancellationToken);

        if (sku == null)
        {
            _logger.LogWarning("SKU não encontrado para ProductId={ProductId}, ColorId={ColorId}, SizeId={SizeId}", productId, colorId, sizeId);
            return null;
        }

        // Buscar estoque disponível
        Domain.Entities.Stock? stock = await _stockRepository.GetAvailableStockAsync(sku.Id, quantity, cancellationToken);

        if (stock == null)
        {
            _logger.LogWarning("Estoque insuficiente para SKU {SkuId}, quantidade solicitada: {Quantity}", sku.Id, quantity);
            return null;
        }

        return new StockAvailabilityResult
        {
            SkuId = sku.Id,
            StockOfficeId = stock.StockOfficeId,
            AvailableQuantity = stock.AvailableQuantity,
            StockOfficeName = stock.StockOffice.Name
        };
    }

    public async Task ReserveStockAsync(int skuId, int stockOfficeId, int quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reservando estoque: SkuId={SkuId}, StockOfficeId={StockOfficeId}, Quantidade={Quantity}", skuId, stockOfficeId, quantity);

        Domain.Entities.Stock? stock = await _stockRepository.GetBySkuAndOfficeAsync(skuId, stockOfficeId, cancellationToken);

        if (stock == null)
            throw new InvalidOperationException($"Estoque não encontrado para SKU {skuId} na filial {stockOfficeId}");

        stock.Reserve(quantity);
        await _stockRepository.UpdateAsync(stock, cancellationToken);
    }

    public async Task ReleaseReservationAsync(int skuId, int stockOfficeId, int quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Liberando reserva de estoque: SkuId={SkuId}, StockOfficeId={StockOfficeId}, Quantidade={Quantity}", skuId, stockOfficeId, quantity);

        Domain.Entities.Stock? stock = await _stockRepository.GetBySkuAndOfficeAsync(skuId, stockOfficeId, cancellationToken);

        if (stock == null)
            throw new InvalidOperationException($"Estoque não encontrado para SKU {skuId} na filial {stockOfficeId}");

        stock.ReleaseReservation(quantity);
        await _stockRepository.UpdateAsync(stock, cancellationToken);
    }

    public async Task DecreaseStockAsync(int skuId, int stockOfficeId, int quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Baixando estoque: SkuId={SkuId}, StockOfficeId={StockOfficeId}, Quantidade={Quantity}", skuId, stockOfficeId, quantity);

        Domain.Entities.Stock? stock = await _stockRepository.GetBySkuAndOfficeAsync(skuId, stockOfficeId, cancellationToken);

        if (stock == null)
            throw new InvalidOperationException($"Estoque não encontrado para SKU {skuId} na filial {stockOfficeId}");

        stock.Decrease(quantity);
        await _stockRepository.UpdateAsync(stock, cancellationToken);
    }
}

