namespace Tgs.Authentication;

public interface IHashAlgorithmFactory
{
	string DefaultAlgorithmName => "argon2di";

	IHashAlgorithm GetHashAlgorithm(string algorithm);

	IHashAlgorithm GetDefaultHashAlgorithm() => GetHashAlgorithm(DefaultAlgorithmName);
}
