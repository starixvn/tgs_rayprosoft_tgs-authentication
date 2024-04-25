using Tgs.Authentication;
using Tgs.Authentication.ApplicationServices;
using Tgs.SerialNumberGenerator;

namespace Authentication.Core.UnitTests.ApplicationServices;

public class CreateAccountCommandServiceTests
{
    [Fact]
    public async Task CreateAccountCommandService_建立一個帳號()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new CreateAccountCommandService(
            fakeTgsRepository,
            new HashAlgorithmFactory(),
            fakeSerialNumberGenerator);

        var command = new CreateAccountCommand(
            "test",
            Guid.NewGuid(),
            "tester",
            "123",
            "op");

        var snapshotId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator.GenerateAsync()
            .Returns(snapshotId);

        // Act
        _ = await sut.GetAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .AddAccountEventAsync(Arg.Any<AccountEvent>());
        _ = fakeTgsRepository.Received(1)
            .UpsertAccountAsync(
                Arg.Any<AccountSnapshot>(),
                Arg.Is(snapshotId),
                Arg.Any<DateTime>());
    }

    [Fact(Timeout = 1000)]
    public async Task CreateAccountCommandService_建立帳號後會回傳帳號Id()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new CreateAccountCommandService(
            fakeTgsRepository,
            new HashAlgorithmFactory(),
            fakeSerialNumberGenerator);

        var command = new CreateAccountCommand(
            "test",
            Guid.NewGuid(),
            "tester",
            "123",
            "op");

        var snapshotId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator.GenerateAsync()
            .Returns(snapshotId);

        var tcs = new TaskCompletionSource<Guid>();

        fakeTgsRepository.When(
            fake => fake.UpsertAccountAsync(
                Arg.Any<AccountSnapshot>(),
                Arg.Is(snapshotId),
                Arg.Any<DateTime>()))
            .Do(Callback.First(callInfo =>
            {
                var snapshot = callInfo.Arg<AccountSnapshot>();

                if (snapshot is not null)
                    tcs.SetResult(snapshot.Id);
            }));

        // Act
        var actual = await sut.GetAsync(command);
        var insertAccountId = await tcs.Task;

        // Assert
        Assert.Equal(insertAccountId, actual);
    }
}
