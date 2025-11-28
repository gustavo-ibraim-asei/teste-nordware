using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.ShippingCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(o => o.CarrierId);

        builder.Property(o => o.CarrierName)
            .HasMaxLength(100);

        builder.Property(o => o.ShippingTypeId);

        builder.Property(o => o.ShippingType)
            .HasMaxLength(50);

        builder.Property(o => o.EstimatedDeliveryDays)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(o => o.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Optimistic concurrency control
        builder.Property(o => o.RowVersion)
            .IsRowVersion()
            .IsRequired();

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => o.TenantId);
    }
}

