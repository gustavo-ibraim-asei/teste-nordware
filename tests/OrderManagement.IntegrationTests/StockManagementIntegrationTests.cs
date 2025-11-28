using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.DTOs;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Multitenancy;
using OrderManagement.IntegrationTests.Helpers;
using Xunit;

namespace OrderManagement.IntegrationTests;

public class StockManagementIntegrationTests : IClassFixture<WebApplicationFactoryHelper>, IDisposable
{
    private readonly WebApplicationFactoryHelper _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly OrderManagementDbContext _dbContext;

    public StockManagementIntegrationTests(WebApplicationFactoryHelper factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        
        // Set tenant
        ITenantProvider tenantProvider = _scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenant("test-tenant");
    }

    [Fact]
    public async Task StockManagement_CompleteFlow_ShouldSucceed()
    {
        // Arrange - Create Product
        CreateProductDto productDto = new CreateProductDto
        {
            Name = "Camiseta Básica",
            Code = "CAM001",
            Description = "Camiseta de algodão"
        };

        HttpResponseMessage productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        ProductDto? product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();

        // Arrange - Create Color
        CreateColorDto colorDto = new CreateColorDto
        {
            Name = "Preto",
            Code = "BLK"
        };

        HttpResponseMessage colorResponse = await _client.PostAsJsonAsync("/api/colors", colorDto);
        colorResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        ColorDto? color = await colorResponse.Content.ReadFromJsonAsync<ColorDto>();
        color.Should().NotBeNull();

        // Arrange - Create Size
        CreateSizeDto sizeDto = new CreateSizeDto
        {
            Name = "M",
            Code = "M"
        };

        HttpResponseMessage sizeResponse = await _client.PostAsJsonAsync("/api/sizes", sizeDto);
        sizeResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        SizeDto? size = await sizeResponse.Content.ReadFromJsonAsync<SizeDto>();
        size.Should().NotBeNull();

        // Arrange - Create StockOffice
        CreateStockOfficeDto stockOfficeDto = new CreateStockOfficeDto
        {
            Name = "Filial São Paulo",
            Code = "SP01"
        };

        HttpResponseMessage stockOfficeResponse = await _client.PostAsJsonAsync("/api/stockoffices", stockOfficeDto);
        stockOfficeResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        StockOfficeDto? stockOffice = await stockOfficeResponse.Content.ReadFromJsonAsync<StockOfficeDto>();
        stockOffice.Should().NotBeNull();

        // Arrange - Create SKU
        CreateSkuDto skuDto = new CreateSkuDto
        {
            ProductId = product!.Id,
            ColorId = color!.Id,
            SizeId = size!.Id
        };

        HttpResponseMessage skuResponse = await _client.PostAsJsonAsync("/api/skus", skuDto);
        skuResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        SkuDto? sku = await skuResponse.Content.ReadFromJsonAsync<SkuDto>();
        sku.Should().NotBeNull();
        sku!.SkuCode.Should().NotBeNullOrEmpty();
        sku.Barcode.Should().NotBeNullOrEmpty();

        // Act - Create Stock
        CreateStockDto stockDto = new CreateStockDto
        {
            SkuId = sku.Id,
            StockOfficeId = stockOffice!.Id,
            Quantity = 100
        };

        HttpResponseMessage stockCreateResponse = await _client.PostAsJsonAsync("/api/stocks", stockDto);
        
        // Assert - Stock Creation
        stockCreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        StockDto? createdStock = await stockCreateResponse.Content.ReadFromJsonAsync<StockDto>();
        createdStock.Should().NotBeNull();
        createdStock!.SkuId.Should().Be(sku.Id);
        createdStock.StockOfficeId.Should().Be(stockOffice.Id);
        createdStock.Quantity.Should().Be(100);
        createdStock.AvailableQuantity.Should().Be(100);
        createdStock.StockOffice.Should().NotBeNull();
        createdStock.StockOffice!.Name.Should().Be("Filial São Paulo");

        // Act - List Stocks
        HttpResponseMessage stocksListResponse = await _client.GetAsync("/api/stocks");
        stocksListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        List<StockDto>? stocks = await stocksListResponse.Content.ReadFromJsonAsync<List<StockDto>>();
        stocks.Should().NotBeNull();
        stocks!.Should().Contain(s => s.Id == createdStock.Id);

        // Act - Reserve Stock
        ReserveStockDto reserveDto = new ReserveStockDto { Quantity = 10 };
        HttpResponseMessage reserveResponse = await _client.PostAsJsonAsync(
            $"/api/stocks/{createdStock.Id}/reserve", 
            reserveDto);
        
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        StockDto? reservedStock = await reserveResponse.Content.ReadFromJsonAsync<StockDto>();
        reservedStock.Should().NotBeNull();
        reservedStock!.Reserved.Should().Be(10);
        reservedStock.AvailableQuantity.Should().Be(90);

        // Act - Decrease Stock
        DecreaseStockDto decreaseDto = new DecreaseStockDto { Quantity = 5 };
        HttpResponseMessage decreaseResponse = await _client.PostAsJsonAsync(
            $"/api/stocks/{createdStock.Id}/decrease", 
            decreaseDto);
        
        decreaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        StockDto? decreasedStock = await decreaseResponse.Content.ReadFromJsonAsync<StockDto>();
        decreasedStock.Should().NotBeNull();
        decreasedStock!.Quantity.Should().Be(95);
        decreasedStock.AvailableQuantity.Should().Be(85);

        // Act - Update Stock
        UpdateStockDto updateStockDto = new UpdateStockDto { Quantity = 150 };
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync(
            $"/api/stocks/{createdStock.Id}", 
            updateStockDto);
        
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        StockDto? updatedStock = await updateResponse.Content.ReadFromJsonAsync<StockDto>();
        updatedStock.Should().NotBeNull();
        updatedStock!.Quantity.Should().Be(150);
    }

    [Fact]
    public async Task CreateStock_WithDuplicateSkuAndOffice_ShouldReturnBadRequest()
    {
        // Arrange - Setup data
        var product = await CreateProductAsync();
        var color = await CreateColorAsync();
        var size = await CreateSizeAsync();
        var stockOffice = await CreateStockOfficeAsync();
        var sku = await CreateSkuAsync(product.Id, color.Id, size.Id);

        CreateStockDto stockDto = new CreateStockDto
        {
            SkuId = sku.Id,
            StockOfficeId = stockOffice.Id,
            Quantity = 100
        };

        // Create first stock
        HttpResponseMessage firstResponse = await _client.PostAsJsonAsync("/api/stocks", stockDto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Try to create duplicate
        HttpResponseMessage duplicateResponse = await _client.PostAsJsonAsync("/api/stocks", stockDto);
        
        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStocks_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange - Setup data
        var product = await CreateProductAsync();
        var color = await CreateColorAsync();
        var size = await CreateSizeAsync();
        var stockOffice1 = await CreateStockOfficeAsync("Filial SP", "SP01");
        var stockOffice2 = await CreateStockOfficeAsync("Filial RJ", "RJ01");
        var sku = await CreateSkuAsync(product.Id, color.Id, size.Id);

        await CreateStockAsync(sku.Id, stockOffice1.Id, 100);
        await CreateStockAsync(sku.Id, stockOffice2.Id, 50);

        // Act - Filter by SkuId
        HttpResponseMessage response = await _client.GetAsync($"/api/stocks?skuId={sku.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<StockDto>? stocks = await response.Content.ReadFromJsonAsync<List<StockDto>>();
        stocks.Should().NotBeNull();
        stocks!.Should().HaveCount(2);
        stocks.Should().OnlyContain(s => s.SkuId == sku.Id);
    }

    private async Task<ProductDto> CreateProductAsync()
    {
        CreateProductDto dto = new CreateProductDto
        {
            Name = "Produto Teste",
            Code = $"PROD{Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "Descrição"
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ProductDto? product = await response.Content.ReadFromJsonAsync<ProductDto>();
        return product!;
    }

    private async Task<ColorDto> CreateColorAsync()
    {
        CreateColorDto dto = new CreateColorDto
        {
            Name = $"Cor{Guid.NewGuid().ToString().Substring(0, 4)}",
            Code = $"C{Guid.NewGuid().ToString().Substring(0, 3)}"
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/colors", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ColorDto? color = await response.Content.ReadFromJsonAsync<ColorDto>();
        return color!;
    }

    private async Task<SizeDto> CreateSizeAsync()
    {
        CreateSizeDto dto = new CreateSizeDto
        {
            Name = $"T{Guid.NewGuid().ToString().Substring(0, 4)}",
            Code = $"T{Guid.NewGuid().ToString().Substring(0, 3)}"
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/sizes", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        SizeDto? size = await response.Content.ReadFromJsonAsync<SizeDto>();
        return size!;
    }

    private async Task<StockOfficeDto> CreateStockOfficeAsync(string name = "Filial Teste", string code = "FT01")
    {
        CreateStockOfficeDto dto = new CreateStockOfficeDto
        {
            Name = name,
            Code = code
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/stockoffices", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        StockOfficeDto? stockOffice = await response.Content.ReadFromJsonAsync<StockOfficeDto>();
        return stockOffice!;
    }

    private async Task<SkuDto> CreateSkuAsync(int productId, int colorId, int sizeId)
    {
        CreateSkuDto dto = new CreateSkuDto
        {
            ProductId = productId,
            ColorId = colorId,
            SizeId = sizeId
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/skus", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        SkuDto? sku = await response.Content.ReadFromJsonAsync<SkuDto>();
        return sku!;
    }

    private async Task<StockDto> CreateStockAsync(int skuId, int stockOfficeId, int quantity)
    {
        CreateStockDto dto = new CreateStockDto
        {
            SkuId = skuId,
            StockOfficeId = stockOfficeId,
            Quantity = quantity
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/stocks", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        StockDto? stock = await response.Content.ReadFromJsonAsync<StockDto>();
        return stock!;
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _scope.Dispose();
        _client.Dispose();
    }
}

