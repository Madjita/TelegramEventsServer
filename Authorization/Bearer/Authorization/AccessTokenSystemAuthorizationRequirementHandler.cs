using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;



namespace Authorization.Bearer.Authorization;

public enum ClaimKey
{
    UserId,
    HardwareId,
    RandomToken,
    JsonWebTokenId,
    AccessToken,
    exp,
    IsPublic,
    ExpiresAt
};

public class AccessTokenSystemAuthorizationRequirementHandler : AuthorizationHandler<AccessTokenSystemAuthorizationRequirement>
{
    private readonly HttpContext _httpContext;

    public AccessTokenSystemAuthorizationRequirementHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;
    }

#pragma warning disable CS1998
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessTokenSystemAuthorizationRequirement systemAuthorizationRequirement
    )
#pragma warning restore CS1998
    {
        if (context.User.Claims.SingleOrDefault(_ => _.Type == ClaimKey.AccessToken.ToString())?.Value == systemAuthorizationRequirement.AccessToken)
            context.Succeed(systemAuthorizationRequirement);
        else
            context.Fail();
    }
}