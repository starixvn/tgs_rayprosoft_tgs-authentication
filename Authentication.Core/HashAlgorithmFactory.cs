namespace Tgs.Authentication;

internal class HashAlgorithmFactory : IHashAlgorithmFactory
{
	public IHashAlgorithm GetHashAlgorithm(string algorithm)
	{
		return algorithm switch
		{
			"argon2di" => new Argon2idAlgorithm(),
			_ => throw new NotSupportedException($"{algorithm} hash algorithm not support.")
		};
	}
}
