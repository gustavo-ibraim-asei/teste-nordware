namespace OrderManagement.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(string username, string tenantId, List<string> roles);
}



