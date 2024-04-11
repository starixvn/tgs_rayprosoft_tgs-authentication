using Tgs.Authentication;

namespace Authentication.Core.UnitTests;

public class HashAlgorithmFactoryTests
{
	[Fact]
	public void HashAlgorithmFactory_取得密碼雜湊演算法物件()
	{
		// Arrange
		var sut = new HashAlgorithmFactory();

		var algorithmName = "argon2di";

		// Act
		var actual = sut.GetHashAlgorithm(algorithmName);

		// Assert
		Assert.NotNull(actual);
	}

	[Fact]
	public void HashAlgorithmFactory_取得密碼雜湊演算法物件_如果沒有實作會丟出NotSupportedException()
	{
		// Arrange
		var sut = new HashAlgorithmFactory();

		var algorithmName = "aaa";

		// Act
		_ = Assert.Throws<NotSupportedException>(() => sut.GetHashAlgorithm(algorithmName));
	}
}
