using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Client.UnitTests;

public class TgsAuthenticateManagerTests
{
    [Fact]
    public async Task TgsAuthenticateManager_驗證jwt有效性()
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

        _ = fakeOptionsMonitor.Get(Arg.Any<string>())
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

        var sut = new TgsAuthenticateManager(fakeOptionsMonitor, NullLogger<TgsAuthenticateManager>.Instance);

        // Act
        var failures = new List<Exception>();
        var actual = await sut.ValidateTokenAsync(token, BearerTokenDefaults.AuthenticationScheme, failures);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual?.Principal.Identity?.IsAuthenticated);
        Assert.Equal("test:test1", actual?.Principal.Identity?.Name);
    }
}