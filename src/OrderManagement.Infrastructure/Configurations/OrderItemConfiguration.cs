using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.OrderId)
            .IsRequired();

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.SkuId);

        builder.Property(i => i.StockOfficeId);

        // Indexes
        builder.HasIndex(i => i.OrderId);
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.SkuId);
        builder.HasIndex(i => i.StockOfficeId);

        // Relationships
        builder.HasOne<Product>()
            .WithMany(p => p.OrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Sku)
            .WithMany(s => s.OrderItems)
            .HasForeignKey(i => i.SkuId)
            .IsRequired(false) // Tornar opcional para evitar warning com query filter
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.StockOffice)
            .WithMany()
            .HasForeignKey(i => i.StockOfficeId)
            .IsRequired(false) // Tornar opcional para evitar warning com query filter
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship com Order - tornar navegação opcional para evitar warning com query filter
        builder.HasOne<Order>(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


