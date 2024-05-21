using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Google.Api;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Tgs.Authentication.Client;
using Tgs.Authentication.Client.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Tgs.Authentication.IntegrationTests;

public class ClientTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task 整合測試_Web驗證實作()
    {
        // Arrange
        var system = "test";
        var userName = "test1";
        var password = "1234";

        var webBuilder = WebApplication.CreateBuilder();

        _ = webBuilder.Logging
            .AddXUnit(testOutputHelper)
            .AddFilter((_, level) => true);

        var fakeAuthenticationClient = Substitute.For<IAuthenticationClient>();

        _ = webBuilder.Services
            .AddAuthentication()
            .AddCookie()
            .Services
            .AddTgsAuthenticationClient()
            .Services
            .AddOptions<TgsBearerOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure(options => options.TokenValidationParameters.ValidAudiences = [system])
            .Services
            .AddSingleton(fakeAuthenticationClient);

        _ = webBuilder.WebHost.UseTestServer();

        await using var web = webBuilder.Build();

        _ = web.MapGet(
            "/login",
            async (
                HttpContext context,
                [FromServices] IAuthenticationClient authClient,
                [FromServices] TgsAuthenticateManager authMgr,
                [FromQuery] string userName,
                [FromQuery] string password) =>
            {
                var result = await authClient.LoginAsync(
                    system,
                    $"{system}:{userName}",
                    password,
                    context.RequestAborted);

                if (result is null)
                    return Results.Forbid();

                var failures = new List<Exception>();
                var ticket = await authMgr.ValidateTokenAsync(
                    result.AccessToken,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    failures,
                    context.RequestAborted);

                return ticket is null
                    ? Results.Forbid()
                    : Results.SignIn(
                    ticket.Principal,
                    ticket.Properties,
                    CookieAuthenticationDefaults.AuthenticationScheme);
            });

        _ = web.MapGet("/info", (HttpContext context) => context.User.Identity?.Name);

        await web.StartAsync();

        using var cookieContainerHandler = new CookieContainerHandler();
        cookieContainerHandler.InnerHandler = web.GetTestServer().CreateHandler();
        using var http = new HttpClient(cookieContainerHandler);
        http.BaseAddress = new Uri("http://localhost");

        var rsa = RSA.Create();
        var rsaSecurityKey = new RsaSecurityKey(rsa);
        var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var identity = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, $"{system}:{userName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.AccountId, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.SessionId, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.System, system)
            ],
            BearerTokenDefaults.AuthenticationScheme);

        var tokenConfiguration = new TokenConfiguration(
            "https://localhost",
            JsonWebKeyConverter.ConvertFromRSASecurityKey(
                new RsaSecurityKey(rsa.ExportParameters(false))));

        _ = fakeAuthenticationClient.GetJsonWebKeyAsync(Arg.Any<CancellationToken>())
            .Returns(tokenConfiguration);

        _ = fakeAuthenticationClient.LoginAsync(
            Arg.Is(system),
            Arg.Is($"{system}:{userName}"),
            Arg.Is(password),
            Arg.Any<CancellationToken>())
            .Returns(new AccessTokenResponse
            {
                AccessToken = TestsHelpers.CreateJwtTokenString(identity, signingCredentials, TimeProvider.System),
                RefreshToken = Guid.NewGuid().ToString(),
                ExpiresIn = DateTimeOffset.Now.AddMinutes(5).ToUnixTimeSeconds()
            });

        // Act
        var loginResponse = await http.GetAsync($"/login?userName={userName}&password={password}");
        var actual = await http.GetStringAsync("/info");

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal($"{system}:{userName}", actual);
    }

    [Fact]
    public async Task 整合測試_Api驗證實作()
    {
        // Arrange
        var system = "test";
        var userName = "test1";

        var webBuilder = WebApplication.CreateBuilder();

        _ = webBuilder.Logging
            .AddXUnit(testOutputHelper)
            .AddFilter((_, level) => true);

        var fakeAuthenticationClient = Substitute.For<IAuthenticationClient>();

        _ = webBuilder.Services
            .AddTgsAuthenticationClient()
            .Services
            .AddAuthentication()
            .AddTgsBearer(options => options.TokenValidationParameters.ValidAudiences = [system])
            .Services
            .AddSingleton(fakeAuthenticationClient);

        _ = webBuilder.WebHost.UseTestServer();

        await using var web = webBuilder.Build();

        _ = web.MapGet("/info", (HttpContext context) => context.User.Identity?.Name);

        await web.StartAsync();

        using var http = web.GetTestClient();

        var rsa = RSA.Create();
        var rsaSecurityKey = new RsaSecurityKey(rsa);
        var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var identity = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, $"{system}:{userName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.AccountId, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.SessionId, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.System, system)
            ],
            BearerTokenDefaults.AuthenticationScheme);

        var tokenConfiguration = new TokenConfiguration(
            "https://localhost",
            JsonWebKeyConverter.ConvertFromRSASecurityKey(
                new RsaSecurityKey(rsa.ExportParameters(false))));

        _ = fakeAuthenticationClient.GetJsonWebKeyAsync(Arg.Any<CancellationToken>())
            .Returns(tokenConfiguration);

        var token = TestsHelpers.CreateJwtTokenString(identity, signingCredentials, TimeProvider.System);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/info");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await http.SendAsync(request);
        var actual = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal($"{system}:{userName}", actual);
    }
}