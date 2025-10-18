using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using ProductInventoryAPI.Core.Authorization;
using ProductInventoryAPI.Core.Interfaces.Services;
using ProductInventoryAPI.Core.Services;
using ProductInventoryAPI.Core.Services.Authentication;

namespace ProductInventoryAPI.Core.Configuration
{
    /// <summary>
    /// Dependency injection configuration for Core layer services
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Add Core layer services to the dependency injection container
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // Register business services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IAuthService, AuthService>();

            // Register authentication services
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHashingService, PasswordHashingService>();

            // Register authorization handlers
            services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();

            // Add AutoMapper
            services.AddAutoMapper(config =>
            {
                config.AddMaps(typeof(DependencyInjection).Assembly);
            });

            return services;
        }
    }
}