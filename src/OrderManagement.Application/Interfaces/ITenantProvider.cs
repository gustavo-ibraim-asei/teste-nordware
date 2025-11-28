namespace OrderManagement.Application.Interfaces;

/// <summary>
/// Interface para obter o tenant atual
/// Implementação deve estar na camada Infrastructure
/// </summary>
public interface ITenantProvider
{
    string GetCurrentTenant();
    void SetTenant(string tenantId);
}



