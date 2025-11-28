using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderManagement.API.Hubs;
using OrderManagement.API.Middleware;
using OrderManagement.API.Services;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mappings;
using OrderManagement.Application.Services;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.ExternalServices;
using OrderManagement.Infrastructure.Multitenancy;
using OrderManagement.Infrastructure.Repositories;
using OrderManagement.Infrastructure.Services;
using OrderManagement.Messaging.Consumers;
using OrderManagement.Messaging.Publishers;
using Polly;
using Polly.Extensions.Http;
using RabbitMQ.Client;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Management API",
        Version = "v1",
        Description = "API completa para gestão de pedidos de e-commerce com processamento assíncrono, integrações externas, autenticação JWT, multitenancy e sistema de frete completo.",
        Contact = new OpenApiContact
        {
            Name = "Order Management",
            Email = "support@ordermanagement.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT"
        }
    });

    // Incluir comentários XML
    string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Digite 'Bearer' [espaço] e então seu token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=OrderManagement;Username=postgres;Password=postgres";

// Multitenancy
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

builder.Services.AddDbContext<OrderManagementDbContext>((serviceProvider, options) =>
{
    ITenantProvider? tenantProvider = serviceProvider.GetService<ITenantProvider>();
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("OrderManagement.Infrastructure");
    });
    if (tenantProvider != null)
    {
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
});

// Repositories and Unit of Work
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddAutoMapper(typeof(MappingProfile), typeof(ShippingMappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();

// Domain Services
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IOrderFactory, OrderFactory>();
builder.Services.AddScoped<IShippingCalculationService, ShippingCalculationService>();

// API Services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<JwtService>();

// Messaging
builder.Services.AddSingleton<IMessagePublisher, RabbitMQMessagePublisher>();
builder.Services.AddScoped<IdempotentMessageProcessor>();
builder.Services.AddHostedService<OrderCreatedConsumer>();
builder.Services.AddHostedService<OrderStatusChangedConsumer>();

// External Services with Polly
builder.Services.AddHttpClient<ViaCepService>()
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)))
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
builder.Services.AddScoped<ViaCepService>();
builder.Services.AddScoped<ShippingService>();

// Redis Cache (optional)
string redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
});

// JWT Authentication
string jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OrderManagement";
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OrderManagement";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Feature Flags
builder.Services.AddSingleton<IFeatureFlags, FeatureFlags>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres")
    .AddCheck("rabbitmq", () =>
    {
        try
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
            };
            using IConnection connection = factory.CreateConnection();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
        }
    })
    .AddCheck("redis", () =>
    {
        try
        {
            // Simple Redis check - in production, use proper Redis health check
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
        }
    });

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Limite de taxa excedido. Por favor, tente novamente mais tarde.", token);
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

WebApplication app = builder.Build();

// Apply migrations
using (IServiceScope scope = app.Services.CreateScope())
{
    OrderManagementDbContext dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await OrderManagement.Infrastructure.Data.DbInitializer.InitializeAsync(dbContext, logger);
        Log.Information("Migrações do banco de dados aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Erro ao aplicar migrações. O banco de dados pode não estar pronto ainda.");
    }
}

// Configure the HTTP request pipeline
// Swagger disponível em todos os ambientes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
    c.RoutePrefix = "swagger"; // Swagger UI em /swagger
    c.DocumentTitle = "Order Management API - Documentação";
    c.DefaultModelsExpandDepth(-1); // Ocultar modelos por padrão
    c.DisplayRequestDuration();
    c.EnableFilter();
    c.EnableDeepLinking();
    c.EnableValidator();
});

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/orderHub");
app.MapHealthChecks("/health");

// Helper methods for Polly policies
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }

