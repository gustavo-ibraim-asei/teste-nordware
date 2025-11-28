using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Configurations;
using OrderManagement.Infrastructure.Multitenancy;

namespace OrderManagement.Infrastructure.Data;

public class OrderManagementDbContext : DbContext
{
    private readonly ITenantProvider? _tenantProvider;

    public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options)
        : base(options)
    {
    }

    public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());

        // Configure ProcessedMessage
        modelBuilder.Entity<ProcessedMessage>(entity =>
        {
            entity.ToTable("ProcessedMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.MessageId).IsUnique();
            entity.HasIndex(e => new { e.MessageId, e.TenantId });
        });

        // Configure Address as owned entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.OwnsOne(o => o.ShippingAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("ShippingStreet").IsRequired();
                address.Property(a => a.Number).HasColumnName("ShippingNumber").IsRequired();
                address.Property(a => a.Complement).HasColumnName("ShippingComplement");
                address.Property(a => a.Neighborhood).HasColumnName("ShippingNeighborhood").IsRequired();
                address.Property(a => a.City).HasColumnName("ShippingCity").IsRequired();
                address.Property(a => a.State).HasColumnName("ShippingState").IsRequired();
                address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").IsRequired();
            });

            // Global filter for multitenancy
            entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default"));
        });
    }

    public override int SaveChanges()
    {
        ApplyTenantFilter();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantFilter();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantFilter()
    {
        if (_tenantProvider == null) return;

        string tenantId = _tenantProvider.GetCurrentTenant();

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added)
            {
                entry.Entity.TenantId = tenantId;
            }
        }
    }
}

