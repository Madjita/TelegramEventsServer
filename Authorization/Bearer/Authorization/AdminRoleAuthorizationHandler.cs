using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TelegramEvents.Fasad;

namespace Authorization.Bearer.Authorization;

public class AdminRoleAuthorizationHandler : AuthorizationHandler<AdminRoleRequirement>
{
    private readonly HttpContext _httpContext;
    private readonly IUserFacade _userFacade;
    private static List<string> _typesToFind = new List<string> { "SessionUID", "UserId", "Role" };
    public AdminRoleAuthorizationHandler(IHttpContextAccessor httpContextAccessor,IServiceScopeFactory scopeFactory)
    {
        _httpContext = httpContextAccessor.HttpContext;
        _userFacade = scopeFactory.CreateScope().ServiceProvider.GetService<IUserFacade>()!;
    }
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRoleRequirement requirement)
    {
        
        if (!context.User.Identity.IsAuthenticated)
        {
            // Пользователь не аутентифицирован, отклоняем запрос
            context.Fail();
            return;
        }
        
        var userClaims = context.User.Claims
            .Where(c => _typesToFind.Contains(c.Type))
            .ToDictionary(c => c.Type, c => c.Value);

        if (_typesToFind.All(type => userClaims.ContainsKey(type)))
        {
            var sessionUIDClaim = userClaims["SessionUID"];
            var userIdClaim = userClaims["UserId"];
            var roleClaim = userClaims["Role"];
            
            long? userId = !string.IsNullOrWhiteSpace(userIdClaim) ? Convert.ToInt64(userIdClaim) : null;
            
            if (!string.IsNullOrEmpty(roleClaim) && requirement.Roles.Contains(roleClaim))
            {
                // Пользователь имеет необходимую роль, отмечаем требование как выполненное
            
                if (string.IsNullOrEmpty(sessionUIDClaim) || string.IsNullOrEmpty(userIdClaim))
                {
                    context.Fail();
                    return;
                }
            
                //ищем пользователя в базе.
                var user = await _userFacade.LogInAsync(sessionUIDClaim, userId);
                if (user is null || !requirement.Roles.Contains(user.Role))
                {
                    context.Fail();
                    return;
                }
            
                context.Succeed(requirement);
                return;
            }
        }
        
        // Пользователь не имеет необходимой роли, отклоняем запрос
        context.Fail();
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
