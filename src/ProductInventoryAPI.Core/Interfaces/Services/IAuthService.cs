using ProductInventoryAPI.Models.DTOs.Auth;
using ProductInventoryAPI.Models.DTOs.Users;
using ProductInventoryAPI.Shared.Common;
using UserDto = ProductInventoryAPI.Models.DTOs.Users.UserDto;

namespace ProductInventoryAPI.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for Authentication and Authorization
    /// </summary>
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<ApiResponse<UserDto>> GetUserProfileAsync(int userId);
        Task<ApiResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto);
    }
}