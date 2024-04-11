using Tgs.Authentication;
using Tgs.Authentication.ApplicationServices;

namespace Authentication.Core.UnitTests.ApplicationServices;
public record GetAccountQueryServiceTests
{
    [Fact]
    public async Task GetAccountQueryService_依據Id取得帳號資訊()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new GetAccountQueryService(fakeTgsRepository);

        var query = new GetAccountQuery(Guid.NewGuid());

        // Act
        _ = await sut.GetAsync(query);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .GetAccountAsync(Arg.Is(query.AccountId));
    }
}
