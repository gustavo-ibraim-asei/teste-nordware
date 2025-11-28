using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class SkuConfiguration : IEntityTypeConfiguration<Sku>
{
    public void Configure(EntityTypeBuilder<Sku> builder)
    {
        builder.ToTable("Skus");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.ProductId)
            .IsRequired();

        builder.Property(s => s.ColorId)
            .IsRequired();

        builder.Property(s => s.SizeId)
            .IsRequired();

        builder.Property(s => s.Barcode)
            .HasMaxLength(100);

        builder.Property(s => s.SkuCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Unique index on SkuCode
        builder.HasIndex(s => s.SkuCode)
            .IsUnique();

        // Indexes
        builder.HasIndex(s => s.ProductId);
        builder.HasIndex(s => s.ColorId);
        builder.HasIndex(s => s.SizeId);
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.Barcode)
            .HasFilter("[Barcode] IS NOT NULL");

        // Composite index for quick lookup
        builder.HasIndex(s => new { s.ProductId, s.ColorId, s.SizeId });

        // Relationships
        builder.HasOne(s => s.Product)
            .WithMany(p => p.Skus)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Color)
            .WithMany(c => c.Skus)
            .HasForeignKey(s => s.ColorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Size)
            .WithMany(sz => sz.Skus)
            .HasForeignKey(s => s.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Stocks)
            .WithOne(st => st.Sku)
            .HasForeignKey(st => st.SkuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.OrderItems)
            .WithOne(oi => oi.Sku)
            .HasForeignKey(oi => oi.SkuId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

