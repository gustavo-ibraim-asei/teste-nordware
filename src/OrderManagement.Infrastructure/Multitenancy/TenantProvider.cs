using OrderManagement.Application.Interfaces;

namespace OrderManagement.Infrastructure.Multitenancy;

public class TenantProvider : ITenantProvider
{
    private string _currentTenant = "default";

    public string GetCurrentTenant()
    {
        return _currentTenant;
    }

    public void SetTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        _currentTenant = tenantId;
    }
}


