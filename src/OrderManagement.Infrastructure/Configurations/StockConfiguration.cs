using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.SkuId)
            .IsRequired();

        builder.Property(s => s.StockOfficeId)
            .IsRequired();

        builder.Property(s => s.Quantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.Reserved)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Unique composite index: (SkuId, StockOfficeId)
        builder.HasIndex(s => new { s.SkuId, s.StockOfficeId })
            .IsUnique();

        // Indexes
        builder.HasIndex(s => s.SkuId);
        builder.HasIndex(s => s.StockOfficeId);
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.UpdatedAt);

        // Relationships
        builder.HasOne(s => s.Sku)
            .WithMany(sku => sku.Stocks)
            .HasForeignKey(s => s.SkuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.StockOffice)
            .WithMany(so => so.Stocks)
            .HasForeignKey(s => s.StockOfficeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



