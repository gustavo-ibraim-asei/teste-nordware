using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Configurations;

namespace OrderManagement.Infrastructure.Data;

/// <summary>
/// DbContext otimizado para leituras (Read Replicas)
/// Usado apenas para queries, nunca para escritas
/// Melhora escalabilidade separando leituras de escritas
/// </summary>
public class OrderManagementReadDbContext : DbContext
{
    private readonly ITenantProvider? _tenantProvider;

    public OrderManagementReadDbContext(DbContextOptions<OrderManagementReadDbContext> options)
        : base(options)
    {
    }

    public OrderManagementReadDbContext(DbContextOptions<OrderManagementReadDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // Apenas DbSets para leitura
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<StockOffice> StockOffices { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Size> Sizes { get; set; }
    public DbSet<Sku> Skus { get; set; }
    public DbSet<Stock> Stocks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar mesmas configurações do contexto principal
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new StockOfficeConfiguration());
        modelBuilder.ApplyConfiguration(new ColorConfiguration());
        modelBuilder.ApplyConfiguration(new SizeConfiguration());
        modelBuilder.ApplyConfiguration(new SkuConfiguration());
        modelBuilder.ApplyConfiguration(new StockConfiguration());

        // Global query filters para multitenancy
        modelBuilder.Entity<Order>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<OrderItem>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<User>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<Role>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<Product>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<StockOffice>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<Color>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<Size>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<Sku>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
        modelBuilder.Entity<Stock>(entity => { entity.HasQueryFilter(o => o.TenantId == (_tenantProvider != null ? _tenantProvider.GetCurrentTenant() : "default")); });
    }

    // Sobrescrever SaveChanges para prevenir escritas acidentais
    public override int SaveChanges() => throw new InvalidOperationException("ReadDbContext é apenas para leitura. Use OrderManagementDbContext para escritas.");
    public override int SaveChanges(bool acceptAllChangesOnSuccess) => throw new InvalidOperationException("ReadDbContext é apenas para leitura. Use OrderManagementDbContext para escritas.");
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("ReadDbContext é apenas para leitura. Use OrderManagementDbContext para escritas.");
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => throw new InvalidOperationException("ReadDbContext é apenas para leitura. Use OrderManagementDbContext para escritas.");
}

