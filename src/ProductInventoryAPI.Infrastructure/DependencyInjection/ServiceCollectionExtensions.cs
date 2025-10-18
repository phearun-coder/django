using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Infrastructure.Configuration;
using ProductInventoryAPI.Infrastructure.Repositories;
using ProductInventoryAPI.Infrastructure.Repositories.Contracts;

namespace ProductInventoryAPI.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Dependency injection configuration for Infrastructure layer
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Configure database
            services.AddDatabaseConfiguration(configuration);

            // Register hybrid ORM services
            services.AddScoped<IStoredProcedureRepository, StoredProcedureRepository>();

            // Register repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            
            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}