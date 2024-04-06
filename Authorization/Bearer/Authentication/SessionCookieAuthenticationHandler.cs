using System.Security.Claims;
using System.Text.Encodings.Web;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using TelegramEvents.Fasad;

namespace Authorization.Bearer.Authentication;

public class SessionCookieAuthenticationSchemeOptions : CookieAuthenticationOptions
{
}

public class SessionCookieAuthenticationHandler : AuthenticationHandler<SessionCookieAuthenticationSchemeOptions>
{
    private readonly SessionCookieAuthenticationSchemeOptions _sessionCookieAuthenticationSchemeOptions;
    private readonly IBucketProvider _bucketProvider;
    private readonly IUserFacade _userFacade;
    
    public SessionCookieAuthenticationHandler(
        IOptionsMonitor<SessionCookieAuthenticationSchemeOptions> options, 
        ILoggerFactory logger,
        UrlEncoder encoder, 
        ISystemClock clock,
        IBucketProvider bucketProvider,
        IUserFacade userFacade
    ) : base(options, logger, encoder, clock)
    {
        _sessionCookieAuthenticationSchemeOptions = options.Get(AuthenticationSchemes.SessionCookie);
        _bucketProvider = bucketProvider;
        _userFacade = userFacade;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var executingEndpoint = Context.GetEndpoint();

            if (executingEndpoint == null)
                return AuthenticateResult.Fail(new NullReferenceException(nameof(executingEndpoint)));

            if (executingEndpoint.Metadata.OfType<AllowAnonymousAttribute>().Any())
                return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), Scheme.Name));
        
            // Проверка аутентификации пользователя
            var result = await Context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
            if (result.Succeeded)
            {
                var claimsPrincipal = result.Principal;
                var claims = claimsPrincipal.Claims;

                var userNameClaim = claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
                var firstNameClaim = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
                var sessionUIDClaim = claims.FirstOrDefault(c => c.Type == "SessionUID")?.Value;
                
                // // Создаем утверждения (claims) пользователя
                // var claims2 = new List<Claim>
                // {
                //     new Claim("UserName", userNameClaim),
                //     new Claim("FirstName", firstNameClaim),
                //     new Claim("SessionUID", sessionUIDClaim),
                //     new Claim("Role", "Administrator"),
                //     // Другие утверждения, если необходимо
                // };
                //
                if (string.IsNullOrEmpty(sessionUIDClaim) || string.IsNullOrEmpty(userNameClaim))
                {
                    return AuthenticateResult.Fail("SessionCookieNotCorrect");
                }
            
                var claimsIdentity = new ClaimsIdentity(claims, nameof(SessionCookieAuthenticationHandler));
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                // Обработка неудачной аутентификации
                return AuthenticateResult.Fail("SessionCookieNotProvided");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}