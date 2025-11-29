namespace OrderManagement.Domain.Entities;

/// <summary>
/// Representa um SKU (Stock Keeping Unit) - combinação única de Produto + Cor + Tamanho
/// </summary>
public class Sku : BaseEntity
{
    public int ProductId { get; private set; }
    public int ColorId { get; private set; }
    public int SizeId { get; private set; }
    public string? Barcode { get; private set; }
    public string SkuCode { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public virtual Product Product { get; private set; } = null!;
    public virtual Color Color { get; private set; } = null!;
    public virtual Size Size { get; private set; } = null!;
    public virtual ICollection<Stock> Stocks { get; private set; } = new List<Stock>();
    public virtual ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    private Sku() { } // EF Core

    public Sku(Product product, Color color, Size size, string tenantId)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (color == null)
            throw new ArgumentNullException(nameof(color));

        if (size == null)
            throw new ArgumentNullException(nameof(size));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        if (product.TenantId != tenantId)
            throw new ArgumentException("O produto deve pertencer ao mesmo tenant", nameof(product));

        if (color.TenantId != tenantId)
            throw new ArgumentException("A cor deve pertencer ao mesmo tenant", nameof(color));

        if (size.TenantId != tenantId)
            throw new ArgumentException("O tamanho deve pertencer ao mesmo tenant", nameof(size));

        ProductId = product.Id;
        ColorId = color.Id;
        SizeId = size.Id;
        Product = product;
        Color = color;
        Size = size;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
        
        // Gerar código SKU usando composição de texto: ProductCode-ColorCode-SizeCode
        string productCode = product.Code;
        string colorCode = color.Code ?? color.Id.ToString();
        string sizeCode = size.Code ?? size.Id.ToString();
        SkuCode = $"{productCode}-{colorCode}-{sizeCode}";
        
        // Gerar código de barras no formato EAN-13
        Barcode = Helpers.EanGenerator.GenerateEan13FromCodes(productCode, colorCode, sizeCode);
    }

    /// <summary>
    /// Atualiza o código de barras manualmente (se necessário sobrescrever o gerado automaticamente)
    /// Valida se o código está no formato EAN válido
    /// </summary>
    public void UpdateBarcode(string? barcode)
    {
        if (barcode != null)
        {
            if (barcode.Length > 13)
                throw new ArgumentException("O código de barras não pode exceder 13 caracteres para o formato EAN", nameof(barcode));

            // Validar formato EAN se fornecido
            if (!Helpers.EanGenerator.IsValidEan(barcode))
                throw new ArgumentException("O código de barras deve estar em um formato EAN-8 ou EAN-13 válido", nameof(barcode));
        }

        Barcode = barcode;
    }
}

