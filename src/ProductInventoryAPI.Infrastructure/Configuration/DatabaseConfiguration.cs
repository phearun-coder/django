using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductInventoryAPI.Infrastructure.Data;

namespace ProductInventoryAPI.Infrastructure.Configuration;

/// <summary>
/// Database configuration and dependency injection setup
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Configure Entity Framework DbContext and database services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // For development/testing, fall back to InMemory database if SQL Server is not available
        var useInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase", false);

        if (useInMemoryDatabase || string.IsNullOrEmpty(connectionString))
        {
            // Configure In-Memory Database for development/testing
            services.AddDbContext<ProductInventoryDbContext>(options =>
                options.UseInMemoryDatabase("ProductInventoryInMemoryDb")
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors());
        }
        else
        {
            // Configure SQL Server Database
            services.AddDbContext<ProductInventoryDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                })
                .EnableSensitiveDataLogging(configuration.GetValue<bool>("EnableSensitiveDataLogging", false))
                .EnableDetailedErrors(configuration.GetValue<bool>("EnableDetailedErrors", false)));
        }

        // Note: Health checks can be added by installing Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
        // and then calling services.AddHealthChecks().AddDbContextCheck<ProductInventoryDbContext>("database");

        return services;
    }

    /// <summary>
    /// Ensure database is created and migrations are applied
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="configuration">Application configuration</param>
    public static async Task EnsureDatabaseCreatedAsync(
        this IServiceProvider serviceProvider, 
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductInventoryDbContext>();
        
        var useInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase", false);
        
        if (useInMemoryDatabase)
        {
            // For in-memory database, ensure it's created
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            // For SQL Server, apply migrations
            await context.Database.MigrateAsync();
        }
    }

    /// <summary>
    /// Seed initial data if database is empty
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductInventoryDbContext>();
        
        // Check if data already exists
        if (await context.Categories.AnyAsync())
        {
            return; // Data already seeded
        }

        // Add seed data manually for in-memory database or if migrations didn't seed
        await SeedInitialDataAsync(context);
    }

    private static async Task SeedInitialDataAsync(ProductInventoryDbContext context)
    {
        // The seed data is handled in the DbContext OnModelCreating method
        // This method is kept for additional seeding if needed
        await context.SaveChangesAsync();
    }
}