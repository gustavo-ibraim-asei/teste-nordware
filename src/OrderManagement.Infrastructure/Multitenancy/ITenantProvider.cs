namespace OrderManagement.Infrastructure.Multitenancy;

public interface ITenantProvider
{
    string GetCurrentTenant();
    void SetTenant(string tenantId);
}


