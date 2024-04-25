using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Tgs.Authentication;

internal class Argon2idAlgorithm : IHashAlgorithm
{
	public string AlgorithmName => "argon2di";

	public async ValueTask<bool> ValidateAsync(string? password, string? passwordHash, CancellationToken cancellationToken = default)
	{
		if (password is null && passwordHash is null)
		{
			return true;
		}

		if (passwordHash is null)
			return false;

		var splits = passwordHash.Split(".");
		var salt = WebEncoders.Base64UrlDecode(splits[0]);
		var hash = WebEncoders.Base64UrlDecode(splits[1]);

		if (password is null)
			return false;

		var passwordByteArray = Encoding.UTF8.GetBytes(password);

		var alg = CreateAlgorithm(passwordByteArray, salt);
		var computedHash = await alg.GetBytesAsync(256).ConfigureAwait(false);

		return hash.SequenceEqual(computedHash);
	}

	public async ValueTask<string> ComputeHashAsync(string password, CancellationToken cancellationToken = default)
	{
		static byte[] CreateRandomSalt()
		{
			var salt = (Span<byte>)stackalloc byte[256];
			RandomNumberGenerator.Fill(salt);

			return salt.ToArray();
		}

		var salt = CreateRandomSalt();

		var passwordByteArray = Encoding.UTF8.GetBytes(password);

		var alg = CreateAlgorithm(passwordByteArray, salt);

		var hash = await alg.GetBytesAsync(256).ConfigureAwait(false);

		return $"{WebEncoders.Base64UrlEncode(salt)}.{WebEncoders.Base64UrlEncode(hash)}";
	}

	private static Argon2id CreateAlgorithm(byte[] password, byte[] salt)
		=> new([.. password])
		{
			DegreeOfParallelism = 2,
			Iterations = 32,
			MemorySize = 8192,
			Salt = salt
		};
}
