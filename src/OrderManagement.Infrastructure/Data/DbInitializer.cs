using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(OrderManagementDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.MigrateAsync();

            // Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                List<Role> roles = new List<Role>
                {
                    new Role("User", "Usuário padrão do sistema"),
                    new Role("Admin", "Administrador do sistema")
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
                logger.LogInformation("Roles iniciais criadas com sucesso");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inicializar banco de dados");
            throw;
        }
    }
}


