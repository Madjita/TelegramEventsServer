using Authorization.Bearer.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Authorization.Bearer.Authentication;

public class DefaultAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

public class DefaultAuthenticationHandler : AuthenticationHandler<DefaultAuthenticationSchemeOptions>
{
    public DefaultAuthenticationHandler(
        IOptionsMonitor<DefaultAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    ) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // return Task.FromResult(AuthenticateResult.NoResult());

        var claimsIdentity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimKey.IsPublic.ToString(), true.ToString(), ClaimValueTypes.Boolean)
        }, nameof(DefaultAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

