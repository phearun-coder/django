using System.Net;
using System.Net.Http.Json;
using ProductInventoryAPI.IntegrationTests.Helpers;
using ProductInventoryAPI.IntegrationTests.Infrastructure;
using ProductInventoryAPI.Models.DTOs;

namespace ProductInventoryAPI.IntegrationTests.Controllers;

public class AnalyticsControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _adminClient;

    public AnalyticsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = AuthTestHelper.GetAuthenticatedClient(_factory);
        _adminClient = AuthTestHelper.GetAuthenticatedAdminClient(_factory);
    }

    [Fact]
    public async Task GetCategoryStatistics_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/analytics/categories/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetInventorySummary_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/analytics/inventory/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetLowStockAlerts_ShouldReturnOk_WithDefaultThreshold()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/analytics/inventory/low-stock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetLowStockAlerts_ShouldReturnOk_WithCustomThreshold()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/analytics/inventory/low-stock?threshold=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetLowStockAlerts_ShouldReturnBadRequest_WhenThresholdIsInvalid(int threshold)
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/analytics/inventory/low-stock?threshold={threshold}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTopSellingProducts_ShouldReturnOk_WithDefaultParameters()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/analytics/products/top-selling");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTopSellingProducts_ShouldReturnOk_WithCustomParameters()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd");
        var endDate = DateTime.Now.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/v1/analytics/products/top-selling?topCount=3&startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetTopSellingProducts_ShouldReturnBadRequest_WhenTopCountIsInvalid(int topCount)
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/analytics/products/top-selling?topCount={topCount}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTopSellingProducts_ShouldReturnBadRequest_WhenStartDateIsAfterEndDate()
    {
        // Arrange
        var startDate = DateTime.Now.ToString("yyyy-MM-dd");
        var endDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/v1/analytics/products/top-selling?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDashboardSummary_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/analytics/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UnauthorizedAccess_ShouldReturnUnauthorized()
    {
        // Arrange
        var unauthorizedClient = _factory.CreateClient();

        // Act
        var response = await unauthorizedClient.GetAsync("/api/v1/analytics/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AnalyticsEndpoints_ShouldRequireAuthentication()
    {
        // Arrange
        var unauthorizedClient = _factory.CreateClient();
        var endpoints = new[]
        {
            "/api/v1/analytics/categories/statistics",
            "/api/v1/analytics/inventory/summary",
            "/api/v1/analytics/inventory/low-stock",
            "/api/v1/analytics/products/top-selling",
            "/api/v1/analytics/dashboard"
        };

        foreach (var endpoint in endpoints)
        {
            // Act
            var response = await unauthorizedClient.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                $"Endpoint {endpoint} should require authentication");
        }
    }

    [Fact]
    public async Task AnalyticsEndpoints_ShouldWorkWithValidToken()
    {
        // Arrange
        var endpoints = new[]
        {
            "/api/v1/analytics/categories/statistics",
            "/api/v1/analytics/inventory/summary",
            "/api/v1/analytics/inventory/low-stock",
            "/api/v1/analytics/products/top-selling",
            "/api/v1/analytics/dashboard"
        };

        foreach (var endpoint in endpoints)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                $"Endpoint {endpoint} should work with valid authentication");
        }
    }
}