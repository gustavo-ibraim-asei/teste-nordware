using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");

        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.ProductId)
            .IsRequired();

        builder.Property(pp => pp.PriceTableId)
            .IsRequired();

        builder.Property(pp => pp.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(pp => pp.CreatedAt)
            .IsRequired();

        builder.Property(pp => pp.UpdatedAt);

        builder.Property(pp => pp.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Índice único para combinação ProductId + PriceTableId
        builder.HasIndex(pp => new { pp.ProductId, pp.PriceTableId })
            .IsUnique();

        // Índices para busca
        builder.HasIndex(pp => pp.ProductId);
        builder.HasIndex(pp => pp.PriceTableId);
        builder.HasIndex(pp => pp.TenantId);

        // Relacionamentos
        builder.HasOne(pp => pp.Product)
            .WithMany(p => p.ProductPrices)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pp => pp.PriceTable)
            .WithMany(pt => pt.ProductPrices)
            .HasForeignKey(pp => pp.PriceTableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

