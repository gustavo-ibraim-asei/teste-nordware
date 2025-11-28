namespace OrderManagement.Application.DTOs;

public class AuthResultDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public int ExpiresIn { get; set; }
}





