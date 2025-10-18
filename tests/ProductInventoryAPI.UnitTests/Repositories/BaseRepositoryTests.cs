using Microsoft.EntityFrameworkCore;
using ProductInventoryAPI.Infrastructure.Data;
using ProductInventoryAPI.Models.Entities;

namespace ProductInventoryAPI.UnitTests.Repositories;

public class BaseRepositoryTests : IDisposable
{
    private readonly ProductInventoryDbContext _context;

    public BaseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductInventoryDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var category = new Category
        {
            Name = "Test Category",
            Description = "Test Description",
            IsActive = true
        };

        // Act
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Assert
        var addedCategory = await _context.Categories.FindAsync(category.Id);
        addedCategory.Should().NotBeNull();
        addedCategory!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Name = "Category 1", Description = "Description 1" },
            new() { Name = "Category 2", Description = "Description 2" }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.Categories.ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Category 1");
        result.Should().Contain(c => c.Name == "Category 2");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var category = new Category
        {
            Name = "Original Name",
            Description = "Original Description"
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        category.Name = "Updated Name";
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        // Assert
        var updatedCategory = await _context.Categories.FindAsync(category.Id);
        updatedCategory!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        // Arrange
        var category = new Category
        {
            Name = "Category to Delete",
            Description = "Will be deleted"
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        var categoryId = category.Id;

        // Act
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        // Assert
        var deletedCategory = await _context.Categories.FindAsync(categoryId);
        deletedCategory.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectEntity()
    {
        // Arrange
        var category = new Category
        {
            Name = "Specific Category",
            Description = "Specific Description"
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.Categories.FindAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Specific Category");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _context.Categories.FindAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task WhereAsync_ShouldFilterEntities()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Name = "Active Category", IsActive = true },
            new() { Name = "Inactive Category", IsActive = false }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        // Act
        var activeCategories = await _context.Categories
            .Where(c => c.IsActive)
            .ToListAsync();

        // Assert
        activeCategories.Should().HaveCount(1);
        activeCategories.First().Name.Should().Be("Active Category");
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}