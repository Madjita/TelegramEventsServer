using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Authorization.Bearer.Authorization;

public class AdminRoleAuthorizationHandler : AuthorizationHandler<AdminRoleRequirement>
{
    private readonly HttpContext _httpContext;

    public AdminRoleAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;
    }
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRoleRequirement requirement)
    {
        
        if (!context.User.Identity.IsAuthenticated)
        {
            // Пользователь не аутентифицирован, отклоняем запрос
            context.Fail();
            return Task.CompletedTask;
        }
        
        string? role = context.User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        
        if (!string.IsNullOrEmpty(role) && requirement.Roles.Contains(role))
        {
            // Пользователь имеет необходимую роль, отмечаем требование как выполненное
            context.Succeed(requirement);
        }
        else
        {
            // Пользователь не имеет необходимой роли, отклоняем запрос
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

public class AdminRoleRequirement : IAuthorizationRequirement
{
    public List<string> Roles { get; }

    public AdminRoleRequirement(List<string> roles)
    {
        Roles = roles;
    }
}
