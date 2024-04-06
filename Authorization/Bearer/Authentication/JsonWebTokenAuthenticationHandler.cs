using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Authorization.Entity;

namespace Authorization.Bearer.Authentication;

public enum QueryKey
{
    JsonWebToken,
    AccessToken
}

public class JsonWebTokenAuthenticationSchemeOptions : JwtBearerOptions
{
}

public class JsonWebTokenAuthenticationHandler : AuthenticationHandler<JsonWebTokenAuthenticationSchemeOptions>
{
    private readonly JsonWebTokenAuthenticationSchemeOptions _jsonWebTokenAuthenticationSchemeOptions;
    private readonly IBucketProvider _bucketProvider;

    public JsonWebTokenAuthenticationHandler(
        IOptionsMonitor<JsonWebTokenAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IBucketProvider bucketProvider
    ) : base(options, logger, encoder, clock)
    {
        _jsonWebTokenAuthenticationSchemeOptions = options.Get(AuthenticationSchemes.JsonWebToken);
        _bucketProvider = bucketProvider;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var executingEndpoint = Context.GetEndpoint();

        if (executingEndpoint == null)
            return AuthenticateResult.Fail(new NullReferenceException(nameof(executingEndpoint)));

        if (executingEndpoint.Metadata.OfType<AllowAnonymousAttribute>().Any())
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), Scheme.Name));

        var authorizationBearerPayloads = new[]
        {
            Context.Request.Headers[HeaderNames.Authorization].SingleOrDefault()?.Split(" ").Last(),
            Context.Request.Query.SingleOrDefault(_ => _.Key == QueryKey.JsonWebToken.ToString()).Value.ToString()
        };

        authorizationBearerPayloads = authorizationBearerPayloads.Where(_ => !string.IsNullOrEmpty(_)).ToArray();

        if (authorizationBearerPayloads.Length == 0)
            return AuthenticateResult.Fail("JsonWebTokenNotProvided");

        var claims = new List<Claim>();

        string? authorizationBearerPayload = null;

        foreach (var authorizationBearerPayloadTemp in authorizationBearerPayloads)
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(authorizationBearerPayloadTemp,
                    _jsonWebTokenAuthenticationSchemeOptions.TokenValidationParameters,
                    out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                claims.AddRange(jwtToken.Claims);

                authorizationBearerPayload = authorizationBearerPayloadTemp;
                break;
            }
            catch (Exception)
            {
                // ignored
            }

        if (string.IsNullOrEmpty(authorizationBearerPayload))
            return AuthenticateResult.Fail("JsonWebTokenIdRetrievalFailed".ToString());


        var bucket = await _bucketProvider.GetBucketAsync("AeroflotData");
        var collection = bucket.DefaultCollection();


        var key = $"JWT_{authorizationBearerPayload}";
        var result = await collection.GetAsync(key);


        var jsonWebToken = result as JsonWebToken;
        if (jsonWebToken == null)
            return AuthenticateResult.Fail("JsonWebTokenNotFound");

        if (jsonWebToken.ExpiresAt < DateTimeOffset.UtcNow)
            return AuthenticateResult.Fail("JsonWebTokenExpired");

        var claimsIdentity = new ClaimsIdentity(claims, nameof(JsonWebTokenAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}