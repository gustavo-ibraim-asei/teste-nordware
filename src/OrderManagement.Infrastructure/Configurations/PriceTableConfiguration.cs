using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class PriceTableConfiguration : IEntityTypeConfiguration<PriceTable>
{
    public void Configure(EntityTypeBuilder<PriceTable> builder)
    {
        builder.ToTable("PriceTables");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pt => pt.Description)
            .HasMaxLength(1000);

        builder.Property(pt => pt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pt => pt.CreatedAt)
            .IsRequired();

        builder.Property(pt => pt.UpdatedAt);

        builder.Property(pt => pt.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Índice único para Name por tenant
        builder.HasIndex(pt => new { pt.Name, pt.TenantId })
            .IsUnique();

        // Índice para IsActive
        builder.HasIndex(pt => pt.IsActive);

        // Relacionamento com ProductPrice
        builder.HasMany(pt => pt.ProductPrices)
            .WithOne(pp => pp.PriceTable)
            .HasForeignKey(pp => pp.PriceTableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

