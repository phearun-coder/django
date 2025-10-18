using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ProductInventoryAPI.Core.Authorization
{
    /// <summary>
    /// Authorization requirement for role-based access control
    /// </summary>
    public class RoleRequirement : IAuthorizationRequirement
    {
        public string[] AllowedRoles { get; }

        public RoleRequirement(params string[] allowedRoles)
        {
            AllowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
        }
    }

    /// <summary>
    /// Authorization handler for role-based access control
    /// </summary>
    public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly ILogger<RoleRequirementHandler> _logger;

        public RoleRequirementHandler(ILogger<RoleRequirementHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RoleRequirement requirement)
        {
            var userRole = context.User.FindFirst("role")?.Value ?? 
                          context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userRole))
            {
                _logger.LogWarning("User has no role claim");
                context.Fail();
                return Task.CompletedTask;
            }

            if (requirement.AllowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogDebug("User with role {Role} granted access", userRole);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User with role {Role} denied access. Required roles: {RequiredRoles}", 
                    userRole, string.Join(", ", requirement.AllowedRoles));
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}