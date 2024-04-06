using Authorization.Bearer.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Authorization.Bearer.Authentication;

public class AccessTokenAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

public class AccessTokenAuthenticationHandler : AuthenticationHandler<AccessTokenAuthenticationSchemeOptions>
{
    public AccessTokenAuthenticationHandler(
        IOptionsMonitor<AccessTokenAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    ) : base(options, logger, encoder, clock)
    {
    }

#pragma warning disable CS1998
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
#pragma warning restore CS1998
    {
        var executingEndpoint = Context.GetEndpoint();

        if (executingEndpoint == null)
            return AuthenticateResult.Fail(new NullReferenceException(nameof(executingEndpoint)));

        if (executingEndpoint.Metadata.OfType<AllowAnonymousAttribute>().Any()
            || executingEndpoint.Metadata.OfType<AllowAnonymousAttribute>().Any())
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), Scheme.Name));

        var authorizationBearerPayloads = new[]
        {
            Context.Request.Headers[HeaderNames.Authorization].SingleOrDefault()?.Split(" ").Last(),
            Context.Request.Query.SingleOrDefault(_ => _.Key == QueryKey.AccessToken.ToString()).Value.ToString()
        };

        authorizationBearerPayloads = authorizationBearerPayloads.Where(_ => !string.IsNullOrEmpty(_)).ToArray();

        if (authorizationBearerPayloads.Length == 0)
            return AuthenticateResult.Fail("AccessTokenNotProvided");

        var accessToken = authorizationBearerPayloads.First();

        // if (string.IsNullOrEmpty(accessToken))
        //     return AuthenticateResult.Fail(Localize.Error.AccessTokenValidationFailed);

        var claims = new List<Claim>
        {
            new(ClaimKey.AccessToken.ToString(), accessToken, ClaimValueTypes.String)
        };

        var claimsIdentity = new ClaimsIdentity(claims, nameof(AccessTokenAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
