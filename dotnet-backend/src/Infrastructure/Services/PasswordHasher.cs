using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Provides password hashing and verification functionality using PBKDF2 with a secure salt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    // The size (in bytes) of the salt and hash key.
    private const int KeySize = 64;

    // The number of PBKDF2 iterations for hashing (higher value increases security and time cost).
    private const int Iterations = 350000;

    // The hash algorithm to use (SHA512) for PBKDF2.
    private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA512;

    /// <summary>
    /// Hashes the given plaintext password using PBKDF2-HMAC-SHA512, a random salt, and a high iteration count.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>
    /// A string containing the hex-encoded hash and the hex-encoded salt, separated by a hyphen ('-').
    /// </returns>
    public string Hash(string password)
    {
        // Generate a cryptographically secure random salt.
        var salt = RandomNumberGenerator.GetBytes(KeySize);

        // Derive the hash using PBKDF2 with the password, salt, iteration count, algorithm and key size.
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            _hashAlgorithm,
            KeySize);

        // Return the hash and salt, both hex-encoded for storage, separated by a hyphen.
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    /// <summary>
    /// Verifies that a given plaintext password corresponds to a stored hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="hash">The stored hash string in format "hash-salt", where both are hex-encoded.</param>
    /// <returns>
    /// True if the password matches the hash (after re-computation); otherwise, false.
    /// </returns>
    public bool Verify(string password, string hash)
    {
        // Split the hash string into hash and salt parts using the hyphen as a delimiter.
        var parts = hash.Split('-');
        if (parts.Length != 2) 
            return false; // The format is invalid.

        // Decode the hex-encoded hash and salt strings.
        var hashBytes = Convert.FromHexString(parts[0]);
        var salt = Convert.FromHexString(parts[1]);

        // Recompute the hash using the provided password and the original salt.
        var newHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            _hashAlgorithm,
            KeySize);

        // Use a time-constant comparison to avoid timing attacks.
        return CryptographicOperations.FixedTimeEquals(hashBytes, newHash);
    }
}
