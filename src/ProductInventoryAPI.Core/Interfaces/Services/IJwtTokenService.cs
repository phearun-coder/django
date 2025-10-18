using ProductInventoryAPI.Models.Entities;
using System.Security.Claims;

namespace ProductInventoryAPI.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for JWT token operations
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generate an access token for the user
        /// </summary>
        /// <param name="user">User to generate token for</param>
        /// <returns>JWT access token</returns>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        /// <returns>Refresh token</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Get claims principal from an expired token (for refresh token validation)
        /// </summary>
        /// <param name="token">Expired JWT token</param>
        /// <returns>Claims principal or null if invalid</returns>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        /// <summary>
        /// Validate a JWT token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// Get token expiration date
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Expiration date</returns>
        DateTime GetTokenExpiration(string token);

        /// <summary>
        /// Extract a specific claim from token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="claimType">Type of claim to extract</param>
        /// <returns>Claim value or null</returns>
        string? GetClaimFromToken(string token, string claimType);
    }
}