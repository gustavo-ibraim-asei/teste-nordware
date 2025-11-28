using Microsoft.AspNetCore.Authorization;

namespace OrderManagement.API.Attributes;

/// <summary>
/// Atributo para indicar que o endpoint requer autenticação JWT
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class JwtRequiredAttribute : AuthorizeAttribute
{
    public JwtRequiredAttribute()
    {
        AuthenticationSchemes = "Bearer";
    }
}


