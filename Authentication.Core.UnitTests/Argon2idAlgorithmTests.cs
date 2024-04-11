using Tgs.Authentication;

namespace Authentication.Core.UnitTests;

public class Argon2idAlgorithmTests
{
    [Fact]
    public async Task Argon2idAlgorithm_可以產生密碼雜湊()
    {
        // Arrange
        var sut = new Argon2idAlgorithm();
        var password = "1234abc";

        // Act
        var actual = await sut.ComputeHashAsync(password);

        // Assert
        Assert.NotEmpty(actual);
    }

    [Fact]
    public async Task Argon2idAlgorithm_產生雜湊每次都不一樣()
    {
        // Arrange
        var sut = new Argon2idAlgorithm();
        var password = "1234abc";

        // Act
        var actual1 = await sut.ComputeHashAsync(password);
        var actual2 = await sut.ComputeHashAsync(password);

        // Assert
        Assert.NotEqual(actual1, actual2);
    }

    [Fact]
    public async Task Argon2idAlgorithm_產生的雜湊可以用來驗證密碼是否正確()
    {
        // Arrange
        var sut = new Argon2idAlgorithm();
        var password = "1234abc";
        var failPassword = "aaaaaaa";
        var hash = await sut.ComputeHashAsync(password);

        // Act
        var actual1 = await sut.ValidateAsync(password, hash);
        var actual2 = await sut.ValidateAsync(failPassword, hash);

        // Assert
        Assert.True(actual1);
        Assert.False(actual2);
    }
}
