using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Client.UnitTests;

public class TgsBearerHandlerTests
{
    [Fact]
    public async Task TgsBearerHandler_處理Token驗證()
    {
        // Arrange
        var rsa = RSA.Create();
        var rsaSecurityKey = new RsaSecurityKey(rsa);
        var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var identity = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, "test:test1"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.AccountId, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.SessionId, Guid.NewGuid().ToString()),
                new Claim(TgsClaimTypes.System, "test")
            ],
            BearerTokenDefaults.AuthenticationScheme);

        var token = TestsHelpers.CreateJwtTokenString(identity, signingCredentials, TimeProvider.System);

        var fakeConfigurationRetriever = Substitute.For<IConfigurationRetriever<TokenConfiguration>>();
        _ = fakeConfigurationRetriever.GetConfigurationAsync(
                Arg.Any<string>(),
                Arg.Any<IDocumentRetriever>(),
                Arg.Any<CancellationToken>())
            .Returns(new TokenConfiguration(
                "https://localhost",
                JsonWebKeyConverter.ConvertFromRSASecurityKey(
                    new RsaSecurityKey(rsa.ExportParameters(false)))));

        var fakeOptionsMonitor = Substitute.For<IOptionsMonitor<TgsBearerOptions>>();
        var authenticateManager = new TgsAuthenticateManager(fakeOptionsMonitor, NullLogger<TgsAuthenticateManager>.Instance);

        var sut = new TgsBearerHandler(
            authenticateManager,
            fakeOptionsMonitor,
            new LoggerFactory(),
            UrlEncoder.Default);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = Substitute.For<IKeyedServiceProvider>()
        };
        var authenticationScheme = new AuthenticationScheme("Test", null, typeof(TgsBearerHandler));

        _ = httpContext.RequestServices.GetRequiredKeyedService<TgsAuthenticateManager>(Arg.Is(authenticationScheme.Name))
            .Returns(authenticateManager);

        _ = fakeOptionsMonitor.Get(Arg.Is(authenticationScheme.Name))
            .Returns(new TgsBearerOptions
            {
                ClaimsIssuer = "https://localhost",
                ConfigurationManager = new ConfigurationManager<TokenConfiguration>("/session/jwk", fakeConfigurationRetriever),
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = ["test"],
                    NameClaimType = ClaimTypes.NameIdentifier
                }
            });

        httpContext.Request.Headers.Authorization = $"Bearer {token}";

        // Act
        await sut.InitializeAsync(authenticationScheme, httpContext);
        var actual = await sut.AuthenticateAsync();

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Succeeded);
    }
}