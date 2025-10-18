using BCrypt.Net;
using Microsoft.Extensions.Logging;
using ProductInventoryAPI.Core.Interfaces.Services;

namespace ProductInventoryAPI.Core.Services.Authentication
{
    /// <summary>
    /// Password hashing service using BCrypt for secure password management
    /// </summary>
    public class PasswordHashingService : IPasswordHashingService
    {
        private readonly ILogger<PasswordHashingService> _logger;

        public PasswordHashingService(ILogger<PasswordHashingService> logger)
        {
            _logger = logger;
        }

        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Password cannot be null or empty", nameof(password));
                }

                // Generate a salt and hash the password with BCrypt
                // WorkFactor 12 provides good security vs performance balance
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
                
                _logger.LogDebug("Password hashed successfully");
                return hashedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Password verification failed: password is null or empty");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(hashedPassword))
                {
                    _logger.LogWarning("Password verification failed: hashed password is null or empty");
                    return false;
                }

                // Verify the password against the hash
                var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                
                _logger.LogDebug("Password verification result: {IsValid}", isValid);
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }

        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Check minimum length
            if (password.Length < 8)
                return false;

            // Check for at least one uppercase letter
            if (!password.Any(char.IsUpper))
                return false;

            // Check for at least one lowercase letter
            if (!password.Any(char.IsLower))
                return false;

            // Check for at least one digit
            if (!password.Any(char.IsDigit))
                return false;

            // Check for at least one special character
            var specialCharacters = "!@#$%^&*(),.?\":{}|<>";
            if (!password.Any(c => specialCharacters.Contains(c)))
                return false;

            return true;
        }

        public string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var chars = new char[length];

            // Ensure at least one character from each required category
            chars[0] = "ABCDEFGHJKLMNOPQRSTUVWXYZ"[random.Next(26)]; // Uppercase
            chars[1] = "abcdefghijklmnopqrstuvwxyz"[random.Next(26)]; // Lowercase
            chars[2] = "0123456789"[random.Next(10)]; // Digit
            chars[3] = "!@#$%^&*"[random.Next(8)]; // Special

            // Fill the rest randomly
            for (int i = 4; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            // Shuffle the array to avoid predictable patterns
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}