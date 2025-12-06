namespace Application.Interfaces;

/// <summary>
/// Provides methods for hashing passwords and verifying hashed passwords,
/// supporting secure password storage and authentication.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a secure hash for the provided plain-text password.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>
    /// A string representing the password hash, including any salt and algorithm information as needed.
    /// </returns>
    string Hash(string password);

    /// <summary>
    /// Verifies that the provided plain-text password, when hashed, matches the specified password hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hash">The hash to compare the password against.</param>
    /// <returns>
    /// <c>true</c> if the password is valid and matches the hash; otherwise, <c>false</c>.
    /// </returns>
    bool Verify(string password, string hash);
}
