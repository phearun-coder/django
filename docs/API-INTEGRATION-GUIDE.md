# Product Inventory API - Integration Guide

## Overview

This guide provides comprehensive information for integrating with the Product Inventory API, including authentication, endpoints, request/response formats, and error handling.

## Table of Contents

1. [Authentication](#authentication)
2. [Base URL and Versioning](#base-url-and-versioning)
3. [Request/Response Format](#requestresponse-format)
4. [Error Handling](#error-handling)
5. [Rate Limiting](#rate-limiting)
6. [Core Endpoints](#core-endpoints)
7. [Analytics Endpoints](#analytics-endpoints)
8. [Inventory Management](#inventory-management)
9. [Code Examples](#code-examples)
10. [SDKs and Libraries](#sdks-and-libraries)
11. [Testing](#testing)
12. [Support](#support)

## Authentication

The API uses JWT (JSON Web Token) authentication. You must obtain a token before making authenticated requests.

### Obtaining a Token

**Endpoint**: `POST /api/v1/auth/login`

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "yourpassword"
}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-15T10:30:00Z",
    "user": {
      "id": "12345",
      "email": "user@example.com",
      "roles": ["InventoryManager"]
    }
  }
}
```

### Using the Token

Include the JWT token in the `Authorization` header for all authenticated requests:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Refresh

**Endpoint**: `POST /api/v1/auth/refresh`

**Headers**:
```http
Authorization: Bearer <current-token>
```

**Response**:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-15T11:30:00Z"
  }
}
```

## Base URL and Versioning

### Production
```
https://api.productinventory.com/api/v1
```

### Staging
```
https://staging-api.productinventory.com/api/v1
```

### Development
```
https://localhost:5001/api/v1
```

### API Versioning

The API uses URL-based versioning. Current version is `v1`. Future versions will be available at `/api/v2`, etc.

## Request/Response Format

### Content Type

All requests and responses use JSON format:
```http
Content-Type: application/json
```

### Standard Response Structure

All API responses follow this structure:

```json
{
  "success": true|false,
  "data": { /* Response data */ },
  "message": "Optional success/error message",
  "errors": [ /* Array of error details */ ],
  "pagination": { /* For paginated responses */ },
  "timestamp": "2024-01-15T10:30:00Z",
  "correlationId": "uuid-for-tracking"
}
```

### Pagination

Paginated endpoints support these query parameters:

- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)
- `sortBy`: Field to sort by
- `sortDirection`: `asc` or `desc`

**Example**:
```http
GET /api/v1/products?page=2&pageSize=50&sortBy=name&sortDirection=asc
```

**Paginated Response**:
```json
{
  "success": true,
  "data": [ /* Array of items */ ],
  "pagination": {
    "currentPage": 2,
    "pageSize": 50,
    "totalPages": 10,
    "totalItems": 500,
    "hasNextPage": true,
    "hasPreviousPage": true
  }
}
```

## Error Handling

### HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict |
| 422 | Unprocessable Entity |
| 429 | Too Many Requests |
| 500 | Internal Server Error |

### Error Response Format

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "code": "INVALID_FORMAT",
      "message": "Email format is invalid"
    },
    {
      "field": "password",
      "code": "TOO_SHORT",
      "message": "Password must be at least 8 characters"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z",
  "correlationId": "uuid-for-tracking"
}
```

### Common Error Codes

| Code | Description |
|------|-------------|
| `INVALID_FORMAT` | Field format is incorrect |
| `REQUIRED_FIELD` | Required field is missing |
| `NOT_FOUND` | Resource not found |
| `DUPLICATE_VALUE` | Unique constraint violation |
| `INSUFFICIENT_PERMISSIONS` | User lacks required permissions |
| `RATE_LIMIT_EXCEEDED` | Too many requests |

## Rate Limiting

The API implements rate limiting to ensure fair usage:

- **Authenticated users**: 1000 requests per hour
- **Unauthenticated users**: 100 requests per hour
- **Burst limit**: 10 requests per second

### Rate Limit Headers

```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1642248600
```

## Core Endpoints

### Categories

#### List Categories
```http
GET /api/v1/categories
```

**Query Parameters**:
- `includeInactive`: Include inactive categories (default: false)
- `page`, `pageSize`: Pagination
- `search`: Search by name

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Electronics",
      "description": "Electronic devices and accessories",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

#### Create Category
```http
POST /api/v1/categories
```

**Request Body**:
```json
{
  "name": "New Category",
  "description": "Category description"
}
```

#### Update Category
```http
PUT /api/v1/categories/{id}
```

#### Delete Category
```http
DELETE /api/v1/categories/{id}
```

### Products

#### List Products
```http
GET /api/v1/products
```

**Query Parameters**:
- `categoryId`: Filter by category
- `minPrice`, `maxPrice`: Price range filter
- `inStock`: Filter by stock availability
- `search`: Search by name or SKU

#### Get Product Details
```http
GET /api/v1/products/{id}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Smartphone X1",
    "description": "Latest smartphone with advanced features",
    "sku": "PHONE-X1-001",
    "price": 699.99,
    "categoryId": 1,
    "category": {
      "id": 1,
      "name": "Electronics"
    },
    "inventory": {
      "currentStock": 150,
      "reorderPoint": 20,
      "maxStockLevel": 500,
      "isInStock": true
    },
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Create Product
```http
POST /api/v1/products
```

**Request Body**:
```json
{
  "name": "New Product",
  "description": "Product description",
  "sku": "PROD-001",
  "price": 99.99,
  "categoryId": 1,
  "inventory": {
    "currentStock": 100,
    "reorderPoint": 10,
    "maxStockLevel": 200
  }
}
```

#### Update Product
```http
PUT /api/v1/products/{id}
```

#### Delete Product
```http
DELETE /api/v1/products/{id}
```

### Inventory

#### Get Inventory Summary
```http
GET /api/v1/inventory/summary
```

**Response**:
```json
{
  "success": true,
  "data": {
    "totalProducts": 150,
    "totalValue": 125000.00,
    "lowStockItems": 12,
    "outOfStockItems": 3,
    "categories": [
      {
        "categoryId": 1,
        "categoryName": "Electronics",
        "productCount": 45,
        "totalValue": 85000.00,
        "lowStockCount": 5
      }
    ]
  }
}
```

#### Get Stock Alerts
```http
GET /api/v1/inventory/alerts
```

**Query Parameters**:
- `level`: `low`, `out`, or `overstocked`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "productId": 1,
      "productName": "Smartphone X1",
      "sku": "PHONE-X1-001",
      "currentStock": 8,
      "reorderPoint": 20,
      "alertLevel": "low",
      "daysUntilStockout": 5
    }
  ]
}
```

## Analytics Endpoints

### Category Statistics

#### All Categories Statistics
```http
GET /api/v1/analytics/categories/statistics
```

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "categoryId": 1,
      "categoryName": "Electronics",
      "productCount": 45,
      "activeProductCount": 42,
      "totalInventoryValue": 85000.00,
      "averageProductPrice": 566.67,
      "lowStockProductCount": 5,
      "outOfStockProductCount": 1,
      "lastUpdated": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### Single Category Statistics
```http
GET /api/v1/analytics/categories/{id}/statistics
```

#### Categories with Low Stock
```http
GET /api/v1/analytics/categories/low-stock
```

#### Top Categories by Product Count
```http
GET /api/v1/analytics/categories/top-by-product-count
```

**Query Parameters**:
- `limit`: Number of top categories to return (default: 10)

### Bulk Operations

#### Bulk Update Category Status
```http
PUT /api/v1/analytics/categories/bulk-update-status
```

**Request Body**:
```json
{
  "categoryIds": [1, 2, 3],
  "isActive": false
}
```

## Inventory Management

### Record Inventory Movement

```http
POST /api/v1/inventorymovement/movements
```

**Request Body**:
```json
{
  "productId": 1,
  "movementType": "Sale",
  "quantity": -5,
  "notes": "Online order #12345",
  "reference": "ORDER-12345"
}
```

**Movement Types**:
- `Purchase`: Incoming stock
- `Sale`: Outgoing stock
- `Adjustment`: Stock correction
- `Return`: Customer return
- `Transfer`: Internal transfer
- `Waste`: Stock write-off

## Code Examples

### JavaScript/Node.js

```javascript
class ProductInventoryAPI {
  constructor(baseUrl, apiKey) {
    this.baseUrl = baseUrl;
    this.apiKey = apiKey;
    this.token = null;
  }

  async login(email, password) {
    const response = await fetch(`${this.baseUrl}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email, password })
    });

    const data = await response.json();
    if (data.success) {
      this.token = data.data.token;
    }
    return data;
  }

  async getProducts(filters = {}) {
    const queryParams = new URLSearchParams(filters);
    const response = await fetch(`${this.baseUrl}/products?${queryParams}`, {
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      }
    });

    return await response.json();
  }

  async createProduct(productData) {
    const response = await fetch(`${this.baseUrl}/products`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(productData)
    });

    return await response.json();
  }

  async getCategoryStatistics() {
    const response = await fetch(`${this.baseUrl}/analytics/categories/statistics`, {
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      }
    });

    return await response.json();
  }

  async recordInventoryMovement(movementData) {
    const response = await fetch(`${this.baseUrl}/inventorymovement/movements`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(movementData)
    });

    return await response.json();
  }
}

// Usage example
const api = new ProductInventoryAPI('https://api.productinventory.com/api/v1');

async function example() {
  // Login
  await api.login('user@example.com', 'password123');

  // Get products
  const products = await api.getProducts({ 
    categoryId: 1, 
    page: 1, 
    pageSize: 20 
  });

  // Create new product
  const newProduct = await api.createProduct({
    name: 'New Product',
    description: 'Product description',
    sku: 'PROD-001',
    price: 99.99,
    categoryId: 1,
    inventory: {
      currentStock: 100,
      reorderPoint: 10,
      maxStockLevel: 200
    }
  });

  // Get analytics
  const stats = await api.getCategoryStatistics();

  // Record inventory movement
  await api.recordInventoryMovement({
    productId: newProduct.data.id,
    movementType: 'Purchase',
    quantity: 50,
    notes: 'Initial stock',
    reference: 'PO-2024-001'
  });
}
```

### Python

```python
import requests
import json
from datetime import datetime

class ProductInventoryAPI:
    def __init__(self, base_url, api_key=None):
        self.base_url = base_url
        self.api_key = api_key
        self.token = None
        self.session = requests.Session()

    def login(self, email, password):
        """Authenticate and obtain JWT token"""
        response = self.session.post(
            f"{self.base_url}/auth/login",
            json={"email": email, "password": password}
        )
        data = response.json()
        
        if data.get('success'):
            self.token = data['data']['token']
            self.session.headers.update({
                'Authorization': f'Bearer {self.token}'
            })
        
        return data

    def get_products(self, **filters):
        """Get products with optional filters"""
        response = self.session.get(
            f"{self.base_url}/products",
            params=filters
        )
        return response.json()

    def create_product(self, product_data):
        """Create a new product"""
        response = self.session.post(
            f"{self.base_url}/products",
            json=product_data
        )
        return response.json()

    def get_category_statistics(self):
        """Get analytics for all categories"""
        response = self.session.get(
            f"{self.base_url}/analytics/categories/statistics"
        )
        return response.json()

    def get_inventory_summary(self):
        """Get inventory summary"""
        response = self.session.get(
            f"{self.base_url}/inventory/summary"
        )
        return response.json()

    def record_inventory_movement(self, movement_data):
        """Record an inventory movement"""
        response = self.session.post(
            f"{self.base_url}/inventorymovement/movements",
            json=movement_data
        )
        return response.json()

    def get_stock_alerts(self, level=None):
        """Get stock alerts"""
        params = {}
        if level:
            params['level'] = level
            
        response = self.session.get(
            f"{self.base_url}/inventory/alerts",
            params=params
        )
        return response.json()

# Usage example
def main():
    api = ProductInventoryAPI('https://api.productinventory.com/api/v1')
    
    # Login
    login_result = api.login('user@example.com', 'password123')
    if not login_result.get('success'):
        print("Login failed:", login_result.get('message'))
        return

    # Get products with filters
    products = api.get_products(
        categoryId=1,
        page=1,
        pageSize=20,
        inStock=True
    )

    # Create new product
    new_product = api.create_product({
        'name': 'Python SDK Test Product',
        'description': 'Created via Python SDK',
        'sku': 'PYTHON-001',
        'price': 149.99,
        'categoryId': 1,
        'inventory': {
            'currentStock': 75,
            'reorderPoint': 15,
            'maxStockLevel': 150
        }
    })

    if new_product.get('success'):
        product_id = new_product['data']['id']
        
        # Record inventory movement
        movement = api.record_inventory_movement({
            'productId': product_id,
            'movementType': 'Purchase',
            'quantity': 25,
            'notes': 'Additional stock via Python SDK',
            'reference': 'PO-PYTHON-001'
        })

    # Get analytics
    stats = api.get_category_statistics()
    print("Category Statistics:", json.dumps(stats, indent=2))

    # Get stock alerts
    alerts = api.get_stock_alerts(level='low')
    print("Low Stock Alerts:", json.dumps(alerts, indent=2))

if __name__ == "__main__":
    main()
```

### C#

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ProductInventoryApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string _token;

    public ProductInventoryApiClient(string baseUrl, HttpClient httpClient = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(string email, string password)
    {
        var loginData = new { email, password };
        var response = await PostAsync<LoginResponse>("/auth/login", loginData);
        
        if (response.Success && response.Data != null)
        {
            _token = response.Data.Token;
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        }
        
        return response;
    }

    public async Task<ApiResponse<List<Product>>> GetProductsAsync(ProductFilter filter = null)
    {
        var queryParams = BuildQueryString(filter);
        return await GetAsync<List<Product>>($"/products{queryParams}");
    }

    public async Task<ApiResponse<Product>> CreateProductAsync(CreateProductRequest product)
    {
        return await PostAsync<Product>("/products", product);
    }

    public async Task<ApiResponse<List<CategoryStatistics>>> GetCategoryStatisticsAsync()
    {
        return await GetAsync<List<CategoryStatistics>>("/analytics/categories/statistics");
    }

    public async Task<ApiResponse<InventorySummary>> GetInventorySummaryAsync()
    {
        return await GetAsync<InventorySummary>("/inventory/summary");
    }

    public async Task<ApiResponse<InventoryMovement>> RecordInventoryMovementAsync(
        CreateInventoryMovementRequest movement)
    {
        return await PostAsync<InventoryMovement>("/inventorymovement/movements", movement);
    }

    private async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1{endpoint}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1{endpoint}", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private string BuildQueryString(object parameters)
    {
        if (parameters == null) return "";

        var properties = parameters.GetType().GetProperties();
        var queryParams = new List<string>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(parameters);
            if (value != null)
            {
                queryParams.Add($"{prop.Name}={Uri.EscapeDataString(value.ToString())}");
            }
        }

        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
    }
}

// Data models
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<ApiError> Errors { get; set; }
    public DateTime Timestamp { get; set; }
    public string CorrelationId { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public User User { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public Inventory Inventory { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Usage example
class Program
{
    static async Task Main(string[] args)
    {
        var client = new ProductInventoryApiClient("https://api.productinventory.com");

        // Login
        var loginResult = await client.LoginAsync("user@example.com", "password123");
        if (!loginResult.Success)
        {
            Console.WriteLine("Login failed: " + loginResult.Message);
            return;
        }

        // Get products
        var products = await client.GetProductsAsync(new ProductFilter
        {
            CategoryId = 1,
            Page = 1,
            PageSize = 20,
            InStock = true
        });

        // Create product
        var newProduct = await client.CreateProductAsync(new CreateProductRequest
        {
            Name = "C# SDK Test Product",
            Description = "Created via C# SDK",
            Sku = "CSHARP-001",
            Price = 199.99m,
            CategoryId = 1,
            Inventory = new CreateInventoryRequest
            {
                CurrentStock = 100,
                ReorderPoint = 20,
                MaxStockLevel = 200
            }
        });

        // Get analytics
        var stats = await client.GetCategoryStatisticsAsync();
        Console.WriteLine($"Retrieved statistics for {stats.Data?.Count} categories");

        // Record inventory movement
        if (newProduct.Success)
        {
            var movement = await client.RecordInventoryMovementAsync(
                new CreateInventoryMovementRequest
                {
                    ProductId = newProduct.Data.Id,
                    MovementType = "Purchase",
                    Quantity = 50,
                    Notes = "Additional stock via C# SDK",
                    Reference = "PO-CSHARP-001"
                });
        }
    }
}
```

## SDKs and Libraries

### Official SDKs

| Language | Repository | Package | Version |
|----------|------------|---------|---------|
| JavaScript/TypeScript | [npm package](https://npmjs.com/package/productinventory-api) | `productinventory-api` | 1.0.0 |
| Python | [PyPI package](https://pypi.org/project/productinventory-api/) | `productinventory-api` | 1.0.0 |
| C# | [NuGet package](https://nuget.org/packages/ProductInventory.Api.Client) | `ProductInventory.Api.Client` | 1.0.0 |

### Installation

#### JavaScript/Node.js
```bash
npm install productinventory-api
```

#### Python
```bash
pip install productinventory-api
```

#### C#
```bash
dotnet add package ProductInventory.Api.Client
```

### Community SDKs

- **PHP**: [productinventory-php](https://github.com/community/productinventory-php)
- **Ruby**: [productinventory-ruby](https://github.com/community/productinventory-ruby)
- **Go**: [productinventory-go](https://github.com/community/productinventory-go)

## Testing

### Test Environment

Use the staging environment for testing:
```
https://staging-api.productinventory.com/api/v1
```

### Test Data

The staging environment includes test data for development and integration testing:

- **Test Categories**: Electronics, Clothing, Books, Sports
- **Test Products**: Various products with different stock levels
- **Test Users**: Different role-based users for testing authorization

### Test Credentials

```json
{
  "admin": {
    "email": "admin@test.com",
    "password": "TestAdmin123!",
    "roles": ["Admin"]
  },
  "manager": {
    "email": "manager@test.com", 
    "password": "TestManager123!",
    "roles": ["InventoryManager"]
  },
  "viewer": {
    "email": "viewer@test.com",
    "password": "TestViewer123!",
    "roles": ["InventoryViewer"]
  }
}
```

### Postman Collection

Download the official Postman collection: [Product Inventory API.postman_collection.json](./postman/Product-Inventory-API.postman_collection.json)

### API Testing Tools

- **Postman**: Official collection with all endpoints
- **Insomnia**: Import our OpenAPI specification
- **curl**: Command-line examples in this documentation
- **Swagger UI**: Available at `/swagger` endpoint

## Support

### Documentation
- **API Reference**: [/docs/api-reference](./API-REFERENCE.md)
- **Getting Started**: [/docs/getting-started](./GETTING-STARTED.md)
- **Troubleshooting**: [/docs/troubleshooting](./TROUBLESHOOTING.md)

### Support Channels
- **Email**: api-support@productinventory.com
- **GitHub Issues**: [Report bugs and request features](https://github.com/your-org/productinventory-api/issues)
- **Community Forum**: [discussions.productinventory.com](https://discussions.productinventory.com)
- **Stack Overflow**: Tag your questions with `productinventory-api`

### SLA and Support Levels

| Level | Response Time | Availability |
|-------|---------------|--------------|
| Production | 2 hours | 99.9% |
| Staging | 8 hours | 99.5% |
| Development | 24 hours | 99% |

### Rate Limits by Plan

| Plan | Requests/Hour | Burst Limit |
|------|---------------|-------------|
| Free | 1,000 | 10/second |
| Professional | 10,000 | 50/second |
| Enterprise | 100,000 | 100/second |

For higher limits or custom plans, contact our sales team at sales@productinventory.com.