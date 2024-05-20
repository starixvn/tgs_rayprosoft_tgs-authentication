using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Tgs.Authentication.Models;

class TgsTokenHandler(
    IOptionsMonitor<TgsTokenOptions> optionsMonitor,
    ILoggerFactory loggerFactory,
    UrlEncoder urlEncoder,
    ITokenRepository tokenRepository,
    TicketDataProtector ticketDataProtector)
    : SignInAuthenticationHandler<TgsTokenOptions>(optionsMonitor, loggerFactory, urlEncoder)
{
    private static readonly AuthenticateResult _FailedReadingJwtToken = AuthenticateResult.Fail("Read token failed");
    private static readonly AuthenticateResult _TokenExpired = AuthenticateResult.Fail("Token expired");
    private static readonly JwtSecurityTokenHandler _JwtSecurityTokenHandler = new();

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = GetBearerTokenOrNull();

        if (token is null)
            return AuthenticateResult.NoResult();

        if (!_JwtSecurityTokenHandler.CanReadToken(token))
            return _FailedReadingJwtToken;

        var jwt = _JwtSecurityTokenHandler.ReadJwtToken(token);

        var utcNow = TimeProvider.GetUtcNow();

        if (utcNow < jwt.ValidFrom || utcNow > jwt.ValidTo)
            return _TokenExpired;

        var jti = Guid.Parse(jwt.Id);
        var tgsTicket = await tokenRepository.GetTicketAsync(
            jti,
            Context.RequestAborted).ConfigureAwait(false);

        if (tgsTicket is null || !tgsTicket.Enabled)
            return _FailedReadingJwtToken;

        var ticketData = ticketDataProtector.Unprotect(tgsTicket.TicketData);
        var ticket = TicketSerializer.Default.Deserialize(ticketData);

        return ticket is null ? _FailedReadingJwtToken : AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.Append(HeaderNames.WWWAuthenticate, "Bearer");
        return base.HandleChallengeAsync(properties);
    }

    protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var utcNow = TimeProvider.GetUtcNow();

        properties ??= new();
        properties.ExpiresUtc = utcNow + Options.BearerTokenExpiration;

        var accessTokenTicket = CreateBearerTicket(user, properties);
        var accessTokenTgsTicket = CreateTgsTicket(TokenType.AccessToken, accessTokenTicket);

        await tokenRepository.SaveTicketAsync(
            accessTokenTgsTicket,
            Context.RequestAborted).ConfigureAwait(false);

        var refreshTokenTicket = CreateRefreshTicket(user, properties, utcNow);
        var refreshTokenTgsTicket = CreateTgsTicket(TokenType.RefreshToken, refreshTokenTicket);

        await tokenRepository.SaveTicketAsync(
            refreshTokenTgsTicket,
            Context.RequestAborted).ConfigureAwait(false);

        var response = new AccessTokenResponse
        {
            AccessToken = CreateJwtTokenString(accessTokenTgsTicket.Id, accessTokenTicket),
            ExpiresIn = (long)Options.BearerTokenExpiration.TotalSeconds,
            RefreshToken = refreshTokenTgsTicket.Id.ToString(),
        };

        Logger.LogInformation("AuthenticationScheme: {AuthenticationScheme} signed in.", Scheme.Name);

        await Context.Response.WriteAsJsonAsync(
            response,
            ResolveAccessTokenJsonTypeInfo(Context)).ConfigureAwait(false);
    }

    private TgsTicket CreateTgsTicket(TokenType tokenType, AuthenticationTicket ticket)
    {
        var protectedTicketData = ticketDataProtector.Protect(TicketSerializer.Default.Serialize(ticket));

        return new TgsTicket(
            Guid.NewGuid(),
            tokenType,
            protectedTicketData,
            ticket.Properties.ExpiresUtc!.Value.UtcDateTime,
            true);
    }

    private static JsonTypeInfo<AccessTokenResponse> ResolveAccessTokenJsonTypeInfo(HttpContext httpContext)
    {
        // Attempt to resolve options from DI then fall back to static options
        var typeInfo = httpContext.RequestServices.GetService<IOptions<JsonOptions>>()
            ?.Value?.SerializerOptions?.GetTypeInfo(typeof(AccessTokenResponse)) as JsonTypeInfo<AccessTokenResponse>;
        return typeInfo ?? TgsTokenJsonSerializerContext.Default.AccessTokenResponse;
    }

    // No-op to avoid interfering with any mass sign-out logic.
    protected override Task HandleSignOutAsync(AuthenticationProperties? properties) => Task.CompletedTask;

    private string? GetBearerTokenOrNull()
    {
        var authorization = Request.Headers.Authorization.ToString();

        return authorization.StartsWith("Bearer ", StringComparison.Ordinal)
            ? authorization["Bearer ".Length..]
            : null;
    }

    private AuthenticationTicket CreateBearerTicket(ClaimsPrincipal user, AuthenticationProperties properties)
        => new(user, properties, $"{Scheme.Name}:AccessToken");

    private AuthenticationTicket CreateRefreshTicket(ClaimsPrincipal user, AuthenticationProperties properties, DateTimeOffset utcNow)
    {
        var refreshProperties = properties.Clone();
        refreshProperties.ExpiresUtc = utcNow + Options.RefreshTokenExpiration;

        return new AuthenticationTicket(user, refreshProperties, $"{Scheme.Name}:RefreshToken");
    }

    private string CreateJwtTokenString(Guid jti, AuthenticationTicket ticket)
    {
        var identity = new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Sub, ticket.Principal.Identity!.Name!),
            new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
            new Claim(TgsClaimTypes.System, ticket.Principal.FindFirstValue(TgsClaimTypes.System) ?? string.Empty),
            new Claim(TgsClaimTypes.AccountId, ticket.Principal.FindFirstValue(TgsClaimTypes.AccountId) ?? string.Empty),
            new Claim(TgsClaimTypes.SessionId, ticket.Principal.FindFirstValue(TgsClaimTypes.SessionId) ?? string.Empty)
        ]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Options.Issuer,
            Subject = identity,
            Expires = ticket.Properties.ExpiresUtc?.LocalDateTime,
            IssuedAt = ticket.Properties.IssuedUtc?.LocalDateTime,
            SigningCredentials = Options.SigningCredentials
        };

        var token = _JwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        return _JwtSecurityTokenHandler.WriteToken(token);
    }
}