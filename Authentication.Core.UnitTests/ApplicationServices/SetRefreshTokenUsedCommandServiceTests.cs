using Tgs.Authentication;
using Tgs.Authentication.ApplicationServices;
using Xunit;

namespace Authentication.Core.UnitTests.ApplicationServices;

public class SetRefreshTokenUsedCommandServiceTests
{
    [Fact]
    public async Task SetRefreshTokenUsedCommandService_設定RefreshToken已使用()
    {
        // Arrange
        var fakeTokenRepository = Substitute.For<ITokenRepository>();

        var sut = new SetRefreshTokenUsedCommandService(fakeTokenRepository);
        var command = new SetRefreshTokenUsedCommand(Guid.NewGuid());

        _ = fakeTokenRepository.GetTicketAsync(Arg.Is(command.RefreshTokenId))
            .Returns(new TgsTicket(
                command.RefreshTokenId,
                TokenType.RefreshToken,
                [],
                DateTime.Now,
                true));

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTokenRepository.Received(1)
            .EnableTicketAsync(Arg.Is(command.RefreshTokenId), Arg.Is(false));
    }

    [Fact]
    public async Task SetRefreshTokenUsedCommandService_設定RefreshToken已使用_如果取得的Ticket不是RefreshToken則不動作()
    {
        // Arrange
        var fakeTokenRepository = Substitute.For<ITokenRepository>();

        var sut = new SetRefreshTokenUsedCommandService(fakeTokenRepository);
        var command = new SetRefreshTokenUsedCommand(Guid.NewGuid());

        _ = fakeTokenRepository.GetTicketAsync(Arg.Is(command.RefreshTokenId))
            .Returns(new TgsTicket(
                command.RefreshTokenId,
                TokenType.AccessToken,
                [],
                DateTime.Now,
                true));

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTokenRepository.DidNotReceive()
            .EnableTicketAsync(Arg.Any<Guid>(), Arg.Any<bool>());
    }
}