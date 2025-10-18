using System.Net;
using System.Net.Http.Json;
using ProductInventoryAPI.IntegrationTests.Helpers;
using ProductInventoryAPI.IntegrationTests.Infrastructure;
using ProductInventoryAPI.Models.DTOs.Categories;

namespace ProductInventoryAPI.IntegrationTests.Controllers;

public class CategoriesControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _adminClient;

    public CategoriesControllerTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = AuthTestHelper.GetAuthenticatedClient(_factory);
        _adminClient = AuthTestHelper.GetAuthenticatedAdminClient(_factory);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOk_WithCategories()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>();
        categories.Should().NotBeNull();
        categories.Should().NotBeEmpty();
        categories.Should().Contain(c => c.Name == "Electronics");
        categories.Should().Contain(c => c.Name == "Clothing");
    }

    [Fact]
    public async Task GetCategory_ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange
        var categoryId = 1; // Electronics

        // Act
        var response = await _client.GetAsync($"/api/v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category.Should().NotBeNull();
        category!.Id.Should().Be(categoryId);
        category.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated_WhenValidData()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Test Category",
            Description = "Test Description"
        };

        // Act
        var response = await _adminClient.PostAsJsonAsync("/api/v1/categories", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdCategory = await response.Content.ReadFromJsonAsync<CategoryDto>();
        createdCategory.Should().NotBeNull();
        createdCategory!.Name.Should().Be("Test Category");
        createdCategory.Description.Should().Be("Test Description");
        createdCategory.IsActive.Should().BeTrue();

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/categories/{createdCategory.Id}");
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "",
            Description = "Test Description"
        };

        // Act
        var response = await _adminClient.PostAsJsonAsync("/api/v1/categories", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnConflict_WhenCategoryNameExists()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Electronics", // Already exists
            Description = "Test Description"
        };

        // Act
        var response = await _adminClient.PostAsJsonAsync("/api/v1/categories", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnForbidden_WhenUserIsNotAdmin()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Test Category",
            Description = "Test Description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/categories", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnOk_WhenValidData()
    {
        // Arrange
        var categoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Clothing
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Clothing",
            Description = "Updated Description",
            IsActive = true
        };

        // Act
        var response = await _adminClient.PutAsJsonAsync($"/api/v1/categories/{categoryId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedCategory = await response.Content.ReadFromJsonAsync<CategoryDto>();
        updatedCategory.Should().NotBeNull();
        updatedCategory!.Name.Should().Be("Updated Clothing");
        updatedCategory.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            IsActive = true
        };

        // Act
        var response = await _adminClient.PutAsJsonAsync($"/api/v1/categories/{nonExistentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNoContent_WhenCategoryExists()
    {
        // Arrange - First create a category to delete
        var createDto = new CreateCategoryDto
        {
            Name = "Category To Delete",
            Description = "Will be deleted"
        };

        var createResponse = await _adminClient.PostAsJsonAsync("/api/v1/categories", createDto);
        var createdCategory = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();

        // Act
        var response = await _adminClient.DeleteAsync($"/api/v1/categories/{createdCategory!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify category is deleted
        var getResponse = await _client.GetAsync($"/api/v1/categories/{createdCategory.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _adminClient.DeleteAsync($"/api/v1/categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetActiveCategories_ShouldReturnOnlyActiveCategories()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/categories/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>();
        categories.Should().NotBeNull();
        categories.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task SearchCategories_ShouldReturnMatchingCategories()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/categories/search?term=Electronics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>();
        categories.Should().NotBeNull();
        categories.Should().Contain(c => c.Name.Contains("Electronics"));
    }

    [Fact]
    public async Task SearchCategories_ShouldReturnEmptyList_WhenNoMatches()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/categories/search?term=NonExistentCategory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>();
        categories.Should().NotBeNull();
        categories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoriesWithCounts_ShouldReturnCategoriesWithProductCounts()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/categories/with-counts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>();
        categories.Should().NotBeNull();
        categories.Should().NotBeEmpty();
        
        // Each category should have a product count
        foreach (var category in categories!)
        {
            category.ProductCount.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task UnauthorizedAccess_ShouldReturnUnauthorized()
    {
        // Arrange
        var unauthorizedClient = _factory.CreateClient();

        // Act
        var response = await unauthorizedClient.GetAsync("/api/v1/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}