using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        builder.ToTable("Sizes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Code)
            .HasMaxLength(50);

        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        // Relationships
        builder.HasMany(s => s.Skus)
            .WithOne(sku => sku.Size)
            .HasForeignKey(sku => sku.SizeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



