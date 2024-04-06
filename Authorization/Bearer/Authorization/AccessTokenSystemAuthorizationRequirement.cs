using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Bearer.Authorization;

public class AccessTokenSystemAuthorizationRequirement : IAuthorizationRequirement
{
    public AccessTokenSystemAuthorizationRequirement(string accessToken)
    {
        AccessToken = accessToken;
    }

    public string AccessToken { get; }
}