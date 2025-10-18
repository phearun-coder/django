using Microsoft.EntityFrameworkCore;
using ProductInventoryAPI.Infrastructure.Data;
using ProductInventoryAPI.Models.Entities;

namespace ProductInventoryAPI.UnitTests.Models;

public class EntityValidationTests : IDisposable
{
    private readonly ProductInventoryDbContext _context;

    public EntityValidationTests()
    {
        var options = new DbContextOptionsBuilder<ProductInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductInventoryDbContext(options);
    }

    [Fact]
    public async Task Category_ShouldCreateValidEntity()
    {
        // Arrange
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic devices and gadgets",
            IsActive = true
        };

        // Act
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Assert
        category.Id.Should().BeGreaterThan(0);
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Product_ShouldCreateValidEntity()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            CategoryId = category.Id,
            SKU = "TEST001",
            IsActive = true
        };

        // Act
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Assert
        product.Id.Should().BeGreaterThan(0);
        product.CategoryId.Should().Be(category.Id);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task User_ShouldCreateValidEntity()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = "User",
            IsActive = true
        };

        // Act
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        user.Id.Should().BeGreaterThan(0);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task InventoryMovement_ShouldCreateValidEntity()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Test Product",
            CategoryId = category.Id,
            SKU = "TEST001",
            Price = 99.99m
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "User"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var movement = new InventoryMovement
        {
            ProductId = product.Id,
            MovementType = "Inbound",
            Quantity = 10,
            PreviousStock = 0,
            NewStock = 10,
            Reason = "Initial Stock",
            CreatedBy = user.Id,
            MovementDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _context.InventoryMovements.AddAsync(movement);
        await _context.SaveChangesAsync();

        // Assert
        movement.Id.Should().BeGreaterThan(0);
        movement.ProductId.Should().Be(product.Id);
        movement.CreatedBy.Should().Be(user.Id);
        movement.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Category_ShouldHaveProductsNavigation()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var products = new List<Product>
        {
            new() { Name = "Product 1", CategoryId = category.Id, SKU = "P001", Price = 10.00m },
            new() { Name = "Product 2", CategoryId = category.Id, SKU = "P002", Price = 20.00m }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var categoryWithProducts = await _context.Categories
            .Include(c => c.Products)
            .FirstAsync(c => c.Id == category.Id);

        // Assert
        categoryWithProducts.Products.Should().HaveCount(2);
        categoryWithProducts.Products.Should().Contain(p => p.Name == "Product 1");
        categoryWithProducts.Products.Should().Contain(p => p.Name == "Product 2");
    }

    [Fact]
    public async Task Product_ShouldHaveCategoryNavigation()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Laptop",
            CategoryId = category.Id,
            SKU = "LAP001",
            Price = 999.99m
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var productWithCategory = await _context.Products
            .Include(p => p.Category)
            .FirstAsync(p => p.Id == product.Id);

        // Assert
        productWithCategory.Category.Should().NotBeNull();
        productWithCategory.Category!.Name.Should().Be("Electronics");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Category_ShouldInitializeWithDefaults(string? description)
    {
        // Act
        var category = new Category
        {
            Name = "Test",
            Description = description
        };

        // Assert
        category.IsActive.Should().BeTrue();
        category.Products.Should().NotBeNull();
        category.Products.Should().BeEmpty();
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}