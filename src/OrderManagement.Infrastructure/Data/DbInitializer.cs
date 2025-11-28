using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Entities;
using System.Data.Common;
using System.Linq;

namespace OrderManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(OrderManagementDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("=== Iniciando inicialização do banco de dados ===");

            // Passo 1: Aguardar conexão com o banco
            int maxRetries = 10;
            int retryCount = 0;
            bool initialConnection = false;

            while (retryCount < maxRetries && !initialConnection)
            {
                try
                {
                    initialConnection = await context.Database.CanConnectAsync();
                    if (!initialConnection)
                    {
                        logger.LogWarning($"Tentativa {retryCount + 1}/{maxRetries}: Aguardando conexão com banco de dados...");
                        await Task.Delay(2000);
                        retryCount++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Tentativa {retryCount + 1}/{maxRetries}: Erro ao conectar. Aguardando...");
                    await Task.Delay(2000);
                    retryCount++;
                }
            }

            if (!initialConnection)
            {
                logger.LogError("Não foi possível conectar ao banco de dados após várias tentativas.");
                return;
            }

            logger.LogInformation("✓ Conexão com banco de dados estabelecida.");

            // Passo 2: Garantir que estamos no schema public
            try
            {
                await context.Database.ExecuteSqlRawAsync("SET search_path TO public;");
                logger.LogInformation("✓ Schema 'public' definido.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Não foi possível definir schema. Continuando...");
            }

            // Passo 3: Verificar se as tabelas existem
            bool tablesExist = false;
            try
            {
                await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Roles\" LIMIT 1");
                tablesExist = true;
                logger.LogInformation("✓ Tabelas já existem no banco de dados.");
            }
            catch
            {
                tablesExist = false;
                logger.LogInformation("⚠ Tabelas não existem. Criando banco de dados...");
            }

            // Passo 4: Aplicar migrations para criar o banco
            if (!tablesExist)
            {
                logger.LogInformation("=== APLICANDO MIGRATIONS PARA CRIAR O BANCO ===");
                
                // Garantir schema public
                try
                {
                    await context.Database.ExecuteSqlRawAsync("SET search_path TO public;");
                }
                catch { }

                try
                {
                    // Garantir que o banco de dados existe
                    logger.LogInformation("Verificando se o banco de dados existe...");
                    bool dbCanConnect = await context.Database.CanConnectAsync();
                    
                    if (!dbCanConnect)
                    {
                        logger.LogWarning("Não foi possível conectar ao banco. Verificando se precisa ser criado...");
                        // Tentar criar o banco usando SQL direto
                        try
                        {
                            var connection = context.Database.GetDbConnection();
                            var dbName = connection.Database;
                            var masterConnectionString = connection.ConnectionString.Replace($"Database={dbName}", "Database=postgres");
                            
                            using (var masterContext = new OrderManagementDbContext(
                                new DbContextOptionsBuilder<OrderManagementDbContext>()
                                    .UseNpgsql(masterConnectionString)
                                    .Options))
                            {
                                // dbName vem do connection string, então é seguro usar diretamente
                                // Não podemos usar parâmetros para CREATE DATABASE
                                var sql = $@"CREATE DATABASE ""{dbName.Replace("\"", "\"\"")}""";
                                await masterContext.Database.ExecuteSqlRawAsync(sql);
                                logger.LogInformation($"✓ Banco de dados '{dbName}' criado.");
                                await Task.Delay(2000);
                            }
                        }
                        catch (Exception createDbEx)
                        {
                            logger.LogWarning(createDbEx, "Não foi possível criar o banco via SQL. Tentando EnsureCreated...");
                            // Fallback: tentar EnsureCreated
                            await context.Database.EnsureCreatedAsync();
                            logger.LogInformation("✓ Banco criado com EnsureCreated.");
                            await Task.Delay(2000);
                        }
                    }
                    else
                    {
                        logger.LogInformation("✓ Conexão com banco estabelecida.");
                    }

                    // Verificar se há migrations pendentes
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                    
                    logger.LogInformation($"Migrations aplicadas: {appliedMigrations.Count()}");
                    if (appliedMigrations.Any())
                    {
                        logger.LogInformation($"Última migration aplicada: {appliedMigrations.Last()}");
                    }
                    logger.LogInformation($"Migrations pendentes: {pendingMigrations.Count()}");
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation($"Próxima migration a aplicar: {pendingMigrations.First()}");
                    }
                    
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Aplicando migrations pendentes...");
                        
                        // Aplicar migrations uma por uma para melhor diagnóstico
                        foreach (var migration in pendingMigrations)
                        {
                            logger.LogInformation($"Aplicando migration: {migration}");
                        }
                        
                        await context.Database.MigrateAsync();
                        logger.LogInformation("✓✓✓ Migrations aplicadas com sucesso!");
                    }
                    else if (!appliedMigrations.Any() && !dbCanConnect)
                    {
                        // Se não há migrations aplicadas nem pendentes e o banco não existia, pode ser que não existam migrations
                        logger.LogWarning("Nenhuma migration encontrada. Banco já foi criado com EnsureCreatedAsync().");
                    }
                    else
                    {
                        logger.LogInformation("Todas as migrations já foram aplicadas.");
                    }
                }
                catch (Exception migrateEx)
                {
                    logger.LogError(migrateEx, "✗ Erro ao aplicar migrations: {Message}", migrateEx.Message);
                    logger.LogError("Tipo do erro: {Type}", migrateEx.GetType().Name);
                    
                    // Se for erro de PostgreSQL, mostrar mais detalhes
                    if (migrateEx.InnerException != null)
                    {
                        logger.LogError("Erro interno: {InnerMessage}", migrateEx.InnerException.Message);
                        if (migrateEx.InnerException.InnerException != null)
                        {
                            logger.LogError("Erro interno 2: {InnerMessage2}", migrateEx.InnerException.InnerException.Message);
                        }
                    }
                    
                    logger.LogError("Stack trace: {StackTrace}", migrateEx.StackTrace);
                    
                    // Tentar fallback para EnsureCreated se Migrate falhar
                    try
                    {
                        logger.LogWarning("Tentando fallback: EnsureCreatedAsync()...");
                        await context.Database.EnsureCreatedAsync();
                        logger.LogInformation("✓✓✓ Banco criado com EnsureCreatedAsync() (fallback)");
                    }
                    catch (Exception ensureEx)
                    {
                        logger.LogError(ensureEx, "✗✗✗ Erro crítico: Nem Migrate nem EnsureCreated funcionaram!");
                        throw new InvalidOperationException(
                            $"Não foi possível criar o banco de dados. Migrate falhou: {migrateEx.Message}. EnsureCreated falhou: {ensureEx.Message}", 
                            migrateEx);
                    }
                }

                // Aguardar
                await Task.Delay(2000);

                // Verificar se foi criado
                bool verified = false;
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Roles\" LIMIT 1");
                        verified = true;
                        logger.LogInformation($"✓✓✓ Tabelas verificadas com sucesso na tentativa {i + 1}!");
                        break;
                    }
                    catch (Exception verifyEx)
                    {
                        if (i < 2)
                        {
                            logger.LogWarning($"Tentativa {i + 1} falhou. Aguardando 2 segundos...");
                            await Task.Delay(2000);
                        }
                        else
                        {
                            logger.LogError(verifyEx, "✗✗✗ ERRO CRÍTICO: Tabelas ainda não existem após aplicar migrations!");
                            throw;
                        }
                    }
                }

                if (!verified)
                {
                    throw new InvalidOperationException("Não foi possível criar as tabelas no banco de dados após aplicar migrations.");
                }
            }
            else
            {
                // Se as tabelas existem, verificar se há migrations pendentes
                try
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation($"Aplicando {pendingMigrations.Count()} migrations pendentes...");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("✓ Migrations pendentes aplicadas com sucesso!");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Erro ao aplicar migrations pendentes (não crítico)");
                }
            }

            // Passo 5: Seed de dados
            await SeedDataAsync(context, logger);

            logger.LogInformation("✓✓✓ Inicialização do banco de dados concluída com sucesso!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗✗✗ ERRO CRÍTICO ao inicializar banco de dados");
            logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            // Não lançar para não impedir a aplicação de iniciar
        }
    }

    private static async Task SeedDataAsync(OrderManagementDbContext context, ILogger logger)
    {
        // Seed Roles
        try
        {
            var rolesCount = await context.Roles.IgnoreQueryFilters().CountAsync();
            if (rolesCount == 0)
            {
                List<Role> roles = new List<Role>
                {
                    new Role("User", "Usuário padrão do sistema"),
                    new Role("Admin", "Administrador do sistema")
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ Roles iniciais criadas com sucesso");
            }
            else
            {
                logger.LogInformation("Roles já existem no banco de dados");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao criar Roles");
        }

        // Seed Colors
        try
        {
            var colorsCount = await context.Colors.IgnoreQueryFilters().CountAsync();
            if (colorsCount == 0)
            {
                string tenantId = "default";
                List<Color> colors = new List<Color>
                {
                    new Color("Preto", "BLK", tenantId),
                    new Color("Branco", "WHT", tenantId),
                    new Color("Azul", "BLU", tenantId),
                    new Color("Vermelho", "RED", tenantId),
                    new Color("Verde", "GRN", tenantId),
                    new Color("Amarelo", "YLW", tenantId)
                };

                await context.Colors.AddRangeAsync(colors);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ Cores básicas criadas com sucesso");
            }
            else
            {
                logger.LogInformation("Colors já existem no banco de dados");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao criar Colors");
        }

        // Seed Sizes
        try
        {
            var sizesCount = await context.Sizes.IgnoreQueryFilters().CountAsync();
            if (sizesCount == 0)
            {
                string tenantId = "default";
                List<Size> sizes = new List<Size>
                {
                    new Size("PP", "PP", tenantId),
                    new Size("P", "P", tenantId),
                    new Size("M", "M", tenantId),
                    new Size("G", "G", tenantId),
                    new Size("GG", "GG", tenantId),
                    new Size("XG", "XG", tenantId)
                };

                await context.Sizes.AddRangeAsync(sizes);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ Tamanhos básicos criados com sucesso");
            }
            else
            {
                logger.LogInformation("Sizes já existem no banco de dados");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao criar Sizes");
        }
    }
}
