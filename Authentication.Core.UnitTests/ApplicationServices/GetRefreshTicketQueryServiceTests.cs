using Tgs.Authentication;
using Tgs.Authentication.ApplicationServices;

namespace Authentication.Core.UnitTests.ApplicationServices;

public class GetRefreshTicketQueryServiceTests
{
    [Fact]
    public async Task GetRefreshTicketQueryService_取得RefreshTokenTicket()
    {
        // Arrange
        var fakeTokenRepository = Substitute.For<ITokenRepository>();

        var sut = new GetRefreshTicketQueryService(fakeTokenRepository);

        var query = new GetRefreshTicketQuery(Guid.NewGuid());

        // Act
        _ = await sut.GetAsync(query);

        // Assert
        _ = fakeTokenRepository.Received(1)
            .GetTicketAsync(Arg.Is(query.RefreshTokenId));
    }

    [Fact]
    public async Task GetRefreshTicketQueryService_如果取得的Ticket不是RefreshToken_會回傳Null()
    {
        // Arrange
        var fakeTokenRepository = Substitute.For<ITokenRepository>();

        var sut = new GetRefreshTicketQueryService(fakeTokenRepository);

        var query = new GetRefreshTicketQuery(Guid.NewGuid());

        _ = fakeTokenRepository.GetTicketAsync(Arg.Is(query.RefreshTokenId))
            .Returns(new TgsTicket(
                query.RefreshTokenId,
                TokenType.AccessToken,
                [],
                DateTime.Today.AddDays(1D),
                true));
        // Act
        var actual = await sut.GetAsync(query);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task GetRefreshTicketQueryService_如果取得的Ticket已經是Disabled_會回傳Null()
    {
        // Arrange
        var fakeTokenRepository = Substitute.For<ITokenRepository>();

        var sut = new GetRefreshTicketQueryService(fakeTokenRepository);

        var query = new GetRefreshTicketQuery(Guid.NewGuid());

        _ = fakeTokenRepository.GetTicketAsync(Arg.Is(query.RefreshTokenId))
            .Returns(new TgsTicket(
                query.RefreshTokenId,
                TokenType.RefreshToken,
                [],
                DateTime.Today.AddDays(1D),
                false));
        // Act
        var actual = await sut.GetAsync(query);

        // Assert
        Assert.Null(actual);
    }
}