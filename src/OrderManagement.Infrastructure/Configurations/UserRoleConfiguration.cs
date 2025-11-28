using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ur => ur.UserId)
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .IsRequired();

        // Unique index
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
    }
}


