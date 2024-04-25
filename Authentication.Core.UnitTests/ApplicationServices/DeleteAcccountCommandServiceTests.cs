using Tgs.Authentication;
using Tgs.Authentication.ApplicationServices;
using Tgs.SerialNumberGenerator;

namespace Authentication.Core.UnitTests.ApplicationServices;

public class DeleteAcccountCommandServiceTests
{
    [Fact]
    public async Task DeleteAcccountCommandService_刪除帳號()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new DeleteAcccountCommandService(fakeTgsRepository, fakeSerialNumberGenerator);

        var command = new DeleteAcccountCommand(Guid.NewGuid(), "op");

        var snapshotId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator.GenerateAsync()
            .Returns(snapshotId);

        _ = fakeTgsRepository.GetAccountAsync(Arg.Is(command.AccountId))
            .Returns(new AccountSnapshot(
                command.AccountId,
                "test:Brand:test1",
                "test",
                "unknown",
                "1234567",
                DateTime.UtcNow));

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .AddAccountEventAsync(Arg.Any<AccountEvent>());
        _ = fakeTgsRepository.Received(1)
            .DeleteAccountAsync(
                Arg.Is(command.AccountId));
    }

    [Fact]
    public async Task DeleteAcccountCommandService_找不到帳號不會動作()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new DeleteAcccountCommandService(fakeTgsRepository, fakeSerialNumberGenerator);

        var command = new DeleteAcccountCommand(Guid.NewGuid(), "op");

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.DidNotReceive()
            .AddAccountEventAsync(Arg.Any<AccountEvent>());
        _ = fakeTgsRepository.DidNotReceive()
            .DeleteAccountAsync(Arg.Any<Guid>());
    }
}
