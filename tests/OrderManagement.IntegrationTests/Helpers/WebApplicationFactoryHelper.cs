using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.API;
using OrderManagement.Application.Interfaces;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Multitenancy;

namespace OrderManagement.IntegrationTests.Helpers;

public class WebApplicationFactoryHelper : WebApplicationFactory<Program>
{
    private readonly string _databaseName;
    private readonly Action<IServiceCollection>? _configureServices;

    public WebApplicationFactoryHelper(string databaseName, Action<IServiceCollection>? configureServices = null)
    {
        _databaseName = databaseName;
        _configureServices = configureServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext
            ServiceDescriptor? descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OrderManagementDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database
            services.AddDbContext<OrderManagementDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Ensure tenant provider is registered
            if (!services.Any(s => s.ServiceType == typeof(ITenantProvider)))
            {
                services.AddScoped<ITenantProvider, TenantProvider>();
            }

            // Allow custom service configuration
            _configureServices?.Invoke(services);
        });

        builder.UseEnvironment("Testing");
    }
}

