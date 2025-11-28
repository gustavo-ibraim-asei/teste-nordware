namespace OrderManagement.API.DTOs;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TenantId { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}


