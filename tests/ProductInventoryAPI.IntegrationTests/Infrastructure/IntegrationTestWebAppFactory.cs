using Microsoft.AspNetCore.Hosting;using Microsoft.AspNetCore.Hosting;

using Microsoft.AspNetCore.Mvc.Testing;using Microsoft.AspNetCore.Mvc.Testing;

using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Logging;using Microsoft.Extensions.Logging;

using ProductInventoryAPI.Infrastructure.Data;using ProductInventoryAPI.Infrastructure.Data;



namespace ProductInventoryAPI.IntegrationTests.Infrastructure;namespace ProductInventoryAPI.IntegrationTests.Infrastructure;



public class IntegrationTestWebAppFactory : WebApplicationFactory<object>public class IntegrationTestWebAppFactory : WebApplicationFactory<object>, IAsyncLifetime

{{

    protected override void ConfigureWebHost(IWebHostBuilder builder)    protected override void ConfigureWebHost(IWebHostBuilder builder)

    {    {

        builder.ConfigureServices(services =>        builder.UseContentRoot(GetWebProjectPath());

        {        

            // Remove the existing DbContext registration        builder.ConfigureServices(services =>

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProductInventoryDbContext>));        {

            if (descriptor != null)            // Remove the existing DbContext registration

                services.Remove(descriptor);            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProductInventoryDbContext>));

            if (descriptor != null)

            // Add DbContext using in-memory database for testing                services.Remove(descriptor);

            services.AddDbContext<ProductInventoryDbContext>(options =>

            {            // Add in-memory database for testing

                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());            services.AddDbContext<ProductInventoryDbContext>(options =>

                options.EnableSensitiveDataLogging();            {

                options.EnableDetailedErrors();                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");

            });                options.EnableSensitiveDataLogging();

                options.EnableDetailedErrors();

            // Configure logging for tests            });

            services.AddLogging(loggingBuilder =>

            {            // Configure logging for tests

                loggingBuilder.AddConsole();            services.AddLogging(builder =>

                loggingBuilder.SetMinimumLevel(LogLevel.Warning);            {

            });                builder.AddConsole();

        });                builder.SetMinimumLevel(LogLevel.Warning);

            });

        builder.UseEnvironment("Testing");        });

    }

        builder.UseEnvironment("Testing");

    public ProductInventoryDbContext GetDbContext()    }

    {

        var scope = Services.CreateScope();    private static string GetWebProjectPath()

        return scope.ServiceProvider.GetRequiredService<ProductInventoryDbContext>();    {

    }        // Get the path to the Web project

}        var currentDirectory = Directory.GetCurrentDirectory();
        var projectPath = Path.Combine(currentDirectory, "..", "..", "src", "ProductInventoryAPI.Web");
        return Path.GetFullPath(projectPath);
    }

    public async Task InitializeAsync()
    {
        // Ensure database is created and seeded
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductInventoryDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        // Seed test data if needed
        await SeedTestDataAsync(context);
    }

    public new async Task DisposeAsync()
    {
        // Clean up
        await base.DisposeAsync();
    }

    private static async Task SeedTestDataAsync(ProductInventoryDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return; // Already seeded

        var categories = new[]
        {
            new ProductInventoryAPI.Models.Entities.Category 
            { 
                Name = "Electronics", 
                Description = "Electronic devices and gadgets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductInventoryAPI.Models.Entities.Category 
            { 
                Name = "Clothing", 
                Description = "Apparel and fashion items",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var products = new[]
        {
            new ProductInventoryAPI.Models.Entities.Product
            {
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                CategoryId = categories[0].Id,
                SKU = "LAP001",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductInventoryAPI.Models.Entities.Product
            {
                Name = "T-Shirt",
                Description = "Cotton t-shirt",
                Price = 29.99m,
                CategoryId = categories[1].Id,
                SKU = "TSH001",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        var users = new[]
        {
            new ProductInventoryAPI.Models.Entities.User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123"),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductInventoryAPI.Models.Entities.User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    public ProductInventoryDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ProductInventoryDbContext>();
    }
}