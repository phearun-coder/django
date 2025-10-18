using System.Security.Claims;

namespace ProductInventoryAPI.Shared.Extensions
{
    /// <summary>
    /// Extension methods for ClaimsPrincipal
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        public static string GetUsername(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        public static string GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public static bool IsInRole(this ClaimsPrincipal principal, string role)
        {
            return principal.IsInRole(role);
        }

        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.IsInRole("Admin");
        }

        public static bool IsManager(this ClaimsPrincipal principal)
        {
            return principal.IsInRole("Manager") || principal.IsInRole("Admin");
        }
    }
}