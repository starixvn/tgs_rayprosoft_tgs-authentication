using Tgs.Authentication;
using Xunit;

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

    [Fact]
    public async Task Argon2idAlgorithm_如果沒有設定密碼_輸入空的密碼會驗證成功()
    {
        // Arrange
        var sut = new Argon2idAlgorithm();
        var password = default(string);
        var hash = default(string);

        // Act
        var actual = await sut.ValidateAsync(password, hash);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task Argon2idAlgorithm_如果有設定密碼_輸入空的密碼會驗證失敗()
    {
        // Arrange
        var sut = new Argon2idAlgorithm();
        var password = default(string);
        var hash = await sut.ComputeHashAsync("1234");

        // Act
        var actual = await sut.ValidateAsync(password, hash);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task Argon2idAlgorithm_如果沒有設定密碼_輸入任何密碼都會驗證失敗()
    {
        // Arrange
        var sut = new Argon2idAlgorithm();
        var password = "1234";
        var hash = default(string);

        // Act
        var actual = await sut.ValidateAsync(password, hash);

        // Assert
        Assert.False(actual);
    }
}
