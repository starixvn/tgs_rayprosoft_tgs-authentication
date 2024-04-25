using Tgs.Authentication;
using Tgs.Authentication.ApplicationServices;

namespace Authentication.Core.UnitTests.ApplicationServices;

public class LoginCommandServiceTests
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:盡可能使用具象類型以提高效能", Justification = "<暫止>")]
    public async Task LoginCommandService_登入_成功回傳帳號資訊()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var hashAlgFactory = new HashAlgorithmFactory() as IHashAlgorithmFactory;

        var sut = new LoginCommandService(
            fakeTgsRepository,
            hashAlgFactory);

        var command = new LoginCommand(
            "test",
            "test1",
            "1234");

        var defaultHasher = hashAlgFactory.GetDefaultHashAlgorithm();
        var passwordHash = await defaultHasher.ComputeHashAsync(command.Password!);

        var account = new Account(
            Guid.NewGuid(),
            command.IdentityName,
            command.System,
            defaultHasher.AlgorithmName,
            passwordHash);

        _ = fakeTgsRepository.GetLoginAccountAsync(
            Arg.Is(command.System),
            Arg.Is(command.IdentityName))
            .Returns(account);

        // Act
        var actual = await sut.GetAsync(command);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(account, actual);
    }
}
