namespace ProductInventoryAPI.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for password hashing and validation operations
    /// </summary>
    public interface IPasswordHashingService
    {
        /// <summary>
        /// Hash a plain text password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verify a plain text password against a hashed password
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hashedPassword">Hashed password to verify against</param>
        /// <returns>True if password matches, false otherwise</returns>
        bool VerifyPassword(string password, string hashedPassword);

        /// <summary>
        /// Check if a password meets strength requirements
        /// </summary>
        /// <param name="password">Password to check</param>
        /// <returns>True if password is strong, false otherwise</returns>
        bool IsPasswordStrong(string password);

        /// <summary>
        /// Generate a random strong password
        /// </summary>
        /// <param name="length">Length of password (default 12)</param>
        /// <returns>Random strong password</returns>
        string GenerateRandomPassword(int length = 12);
    }
}