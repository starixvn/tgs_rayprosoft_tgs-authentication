namespace Tgs.Authentication;

public interface IHashAlgorithm
{
	string AlgorithmName { get; }

	ValueTask<bool> ValidateAsync(string password, string passwordHash, CancellationToken cancellationToken = default);

	ValueTask<string> ComputeHashAsync(string password, CancellationToken cancellationToken = default);
}
