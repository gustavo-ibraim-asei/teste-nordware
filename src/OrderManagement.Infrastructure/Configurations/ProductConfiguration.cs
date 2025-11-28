using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        // Índice único para Code por tenant
        builder.HasIndex(p => new { p.Code, p.TenantId })
            .IsUnique();

        // Índice para Name por tenant (para busca)
        builder.HasIndex(p => new { p.Name, p.TenantId });

        // Relacionamentos já configurados em SkuConfiguration, OrderItemConfiguration e ProductPriceConfiguration
    }
}

