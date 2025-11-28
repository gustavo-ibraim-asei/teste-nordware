using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> builder)
    {
        builder.ToTable("Colors");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Code)
            .HasMaxLength(50);

        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        // Relationships
        builder.HasMany(c => c.Skus)
            .WithOne(s => s.Color)
            .HasForeignKey(s => s.ColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



