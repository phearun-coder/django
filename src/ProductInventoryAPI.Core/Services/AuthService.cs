using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ProductInventoryAPI.Core.Exceptions;
using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Core.Interfaces.Services;
using ProductInventoryAPI.Models.DTOs.Auth;
using ProductInventoryAPI.Models.DTOs.Users;
using ProductInventoryAPI.Models.Entities;
using ProductInventoryAPI.Shared.Common;
using ProductInventoryAPI.Shared.Constants;
using ProductInventoryAPI.Shared.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserDto = ProductInventoryAPI.Models.DTOs.Users.UserDto;

namespace ProductInventoryAPI.Core.Services
{
    /// <summary>
    /// Service implementation for Authentication and Authorization business logic
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AuthService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for username: {Username}", loginDto.Username);

                var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.Username);
                if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for username: {Username}", loginDto.Username);
                    return ApiResponse<AuthResponseDto>.ErrorResult("Invalid username or password.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user: {Username}", loginDto.Username);
                    return ApiResponse<AuthResponseDto>.ErrorResult("Account is inactive.");
                }

                var token = await GenerateJwtTokenAsync(user);
                var refreshToken = GenerateRefreshToken();

                // Update user's last login and refresh token
                user.LastLoginAt = DateTime.UtcNow;
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                var authResponse = new AuthResponseDto
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                    User = userDto
                };

                _logger.LogInformation("User logged in successfully: {Username}", loginDto.Username);
                return ApiResponse<AuthResponseDto>.SuccessResult(authResponse, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for username: {Username}", loginDto.Username);
                return ApiResponse<AuthResponseDto>.ErrorResult("An error occurred during login.");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Registration attempt for username: {Username}", registerDto.Username);

                // Validate registration
                await ValidateRegistrationAsync(registerDto);

                var user = _mapper.Map<User>(registerDto);
                user.PasswordHash = HashPassword(registerDto.Password);
                user.Role = registerDto.Role ?? AppConstants.UserRoles.User; // Default to User role
                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                var createdUser = await _unitOfWork.Users.CreateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var token = await GenerateJwtTokenAsync(createdUser);
                var refreshToken = GenerateRefreshToken();

                // Update user with refresh token
                createdUser.RefreshToken = refreshToken;
                createdUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                await _unitOfWork.Users.UpdateAsync(createdUser);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(createdUser);
                var authResponse = new AuthResponseDto
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                    User = userDto
                };

                _logger.LogInformation("User registered successfully: {Username}", registerDto.Username);
                return ApiResponse<AuthResponseDto>.SuccessResult(authResponse, "Registration successful.");
            }
            catch (ConflictException ex)
            {
                return ApiResponse<AuthResponseDto>.ErrorResult(ex.Message);
            }
            catch (ValidationException ex)
            {
                return ApiResponse<AuthResponseDto>.ErrorResult("Validation failed", ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for username: {Username}", registerDto.Username);
                return ApiResponse<AuthResponseDto>.ErrorResult("An error occurred during registration.");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                _logger.LogInformation("Refresh token attempt");

                var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshTokenDto.RefreshToken);
                if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid or expired refresh token");
                    return ApiResponse<AuthResponseDto>.ErrorResult("Invalid or expired refresh token.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Refresh token attempt for inactive user: {UserId}", user.Id);
                    return ApiResponse<AuthResponseDto>.ErrorResult("Account is inactive.");
                }

                var newToken = await GenerateJwtTokenAsync(user);
                var newRefreshToken = GenerateRefreshToken();

                // Update refresh token
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                var authResponse = new AuthResponseDto
                {
                    AccessToken = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                    User = userDto
                };

                _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);
                return ApiResponse<AuthResponseDto>.SuccessResult(authResponse, "Token refreshed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return ApiResponse<AuthResponseDto>.ErrorResult("An error occurred during token refresh.");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Logout attempt");

                var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken);
                if (user != null)
                {
                    // Clear refresh token
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;

                    await _unitOfWork.Users.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("User logged out successfully: {UserId}", user.Id);
                }

                return ApiResponse<bool>.SuccessResult(true, "Logout successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return ApiResponse<bool>.ErrorResult("An error occurred during logout.");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                _logger.LogInformation("Password change attempt for user: {UserId}", userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException(nameof(User), userId);
                }

                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid current password for user: {UserId}", userId);
                    return ApiResponse<bool>.ErrorResult("Current password is incorrect.");
                }

                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return ApiResponse<bool>.SuccessResult(true, "Password changed successfully.");
            }
            catch (NotFoundException)
            {
                return ApiResponse<bool>.ErrorResult($"User with ID {userId} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password change for user: {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("An error occurred during password change.");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserProfileAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting user profile for user: {UserId}", userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException(nameof(User), userId);
                }

                var userDto = _mapper.Map<UserDto>(user);
                return ApiResponse<UserDto>.SuccessResult(userDto);
            }
            catch (NotFoundException)
            {
                return ApiResponse<UserDto>.ErrorResult($"User with ID {userId} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user profile for user: {UserId}", userId);
                return ApiResponse<UserDto>.ErrorResult("An error occurred while retrieving user profile.");
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating user profile for user: {UserId}", userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException(nameof(User), userId);
                }

                // Validate email uniqueness if changed
                if (!string.Equals(user.Email, updateDto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _unitOfWork.Users.EmailExistsAsync(updateDto.Email, userId);
                    if (emailExists)
                    {
                        return ApiResponse<UserDto>.ErrorResult($"Email '{updateDto.Email}' is already in use.");
                    }
                }

                _mapper.Map(updateDto, user);
                user.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(updatedUser);
                _logger.LogInformation("User profile updated successfully for user: {UserId}", userId);

                return ApiResponse<UserDto>.SuccessResult(userDto, "Profile updated successfully.");
            }
            catch (NotFoundException)
            {
                return ApiResponse<UserDto>.ErrorResult($"User with ID {userId} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user profile for user: {UserId}", userId);
                return ApiResponse<UserDto>.ErrorResult("An error occurred while updating user profile.");
            }
        }

        #region Private Helper Methods

        private async Task ValidateRegistrationAsync(RegisterDto registerDto)
        {
            var errors = new List<string>();

            // Check username uniqueness
            var usernameExists = await _unitOfWork.Users.UsernameExistsAsync(registerDto.Username);
            if (usernameExists)
            {
                errors.Add($"Username '{registerDto.Username}' is already taken.");
            }

            // Check email uniqueness
            var emailExists = await _unitOfWork.Users.EmailExistsAsync(registerDto.Email);
            if (emailExists)
            {
                errors.Add($"Email '{registerDto.Email}' is already registered.");
            }

            // Validate role
            if (!string.IsNullOrEmpty(registerDto.Role) && 
                !AppConstants.UserRoles.AllRoles.Contains(registerDto.Role))
            {
                errors.Add($"Invalid role '{registerDto.Role}'.");
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetJwtSecret());

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = GetJwtIssuer(),
                Audience = GetJwtAudience()
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string GetJwtSecret()
        {
            return _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
        }

        private string GetJwtIssuer()
        {
            return _configuration["Jwt:Issuer"] ?? "ProductInventoryAPI";
        }

        private string GetJwtAudience()
        {
            return _configuration["Jwt:Audience"] ?? "ProductInventoryAPI";
        }

        private int GetJwtExpirationMinutes()
        {
            return _configuration.GetValue<int>("Jwt:ExpirationInMinutes", 30);
        }

        #endregion
    }
}