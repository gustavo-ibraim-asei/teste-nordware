using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class StockOfficeConfiguration : IEntityTypeConfiguration<StockOffice>
{
    public void Configure(EntityTypeBuilder<StockOffice> builder)
    {
        builder.ToTable("StockOffices");

        builder.HasKey(so => so.Id);

        builder.Property(so => so.Id)
            .ValueGeneratedOnAdd();

        builder.Property(so => so.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(so => so.Code)
            .HasMaxLength(50);

        builder.Property(so => so.CreatedAt)
            .IsRequired();

        builder.Property(so => so.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(so => so.TenantId);
        builder.HasIndex(so => so.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        // Relationships
        builder.HasMany(so => so.Stocks)
            .WithOne(s => s.StockOffice)
            .HasForeignKey(s => s.StockOfficeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



