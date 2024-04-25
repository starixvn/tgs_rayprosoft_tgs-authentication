using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tgs.Authentication;
using Tgs.Authentication.ApplicationServices;
using Tgs.SerialNumberGenerator;
using Xunit;

namespace Authentication.Core.UnitTests.ApplicationServices;

public class ChangePasswordCommandServiceTests
{
    [Fact]
    public async Task ChangePasswordCommandService_變更帳號密碼()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new ChangePasswordCommandService(
            fakeTgsRepository,
            new HashAlgorithmFactory(),
            fakeSerialNumberGenerator);

        var command = new ChangePasswordCommand(Guid.NewGuid(), "new1234", "op");

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
            .UpsertAccountAsync(
                Arg.Any<AccountSnapshot>(),
                Arg.Is(snapshotId),
                Arg.Any<DateTime>());
    }

    [Fact]
    public async Task ChangePasswordCommandService_變更帳號密碼_如果找不到帳號就不動作()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new ChangePasswordCommandService(
            fakeTgsRepository,
            new HashAlgorithmFactory(),
            fakeSerialNumberGenerator);

        var command = new ChangePasswordCommand(Guid.NewGuid(), "new1234", "op");

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.DidNotReceive()
            .AddAccountEventAsync(Arg.Any<AccountEvent>());
        _ = fakeTgsRepository.DidNotReceive()
            .UpsertAccountAsync(
                Arg.Any<AccountSnapshot>(),
                Arg.Any<Guid>(),
                Arg.Any<DateTime>());
    }
}
