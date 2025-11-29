namespace OrderManagement.Domain.Entities;

/// <summary>
/// Representa uma filial de estoque onde os produtos são armazenados
/// </summary>
public class StockOffice : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<Stock> Stocks { get; private set; } = new List<Stock>();

    private StockOffice() { } // EF Core

    public StockOffice(string name, string? code, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome não pode ser vazio", nameof(name));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        Name = name;
        Code = code;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome não pode ser vazio", nameof(name));

        Name = name;
    }

    public void UpdateCode(string? code)
    {
        Code = code;
    }
}

