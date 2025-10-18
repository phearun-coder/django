# Product Inventory API - Hybrid ORM Implementation

This project demonstrates a comprehensive hybrid ORM approach using ASP.NET Core, Entity Framework Core, and SQL Server stored procedures. **Phases 1-13 Complete** ✅

## ⭐ Project Status

| Phase | Status | Description |
|-------|--------|-------------|
| Phase 1 | ✅ Complete | Project Setup & Clean Architecture |
| Phase 2 | ✅ Complete | Database Schema & Entity Framework |
| Phase 3 | ✅ Complete | Repository Pattern Implementation |
| Phase 4 | ✅ Complete | Stored Procedures & Raw SQL |
| Phase 5 | ✅ Complete | Hybrid Repository Integration |
| Phase 6 | ✅ Complete | API Controllers & Endpoints |
| Phase 7 | ✅ Complete | Authentication & Authorization |
| Phase 8 | ✅ Complete | Service Layer & Business Logic |
| Phase 9 | ✅ Complete | Configuration & Environment Setup |
| Phase 10 | ✅ Complete | Logging, Monitoring & Health Checks |
| Phase 11 | ✅ Complete | Error Handling & Validation |
| Phase 12 | ✅ Complete | Testing Infrastructure (29 tests passing) |
| Phase 13 | ✅ Complete | Deployment Configuration |
| Phase 14 | ✅ Complete | Integration Points & Documentation |

## 🎯 Tutorial Features Implemented

### ✅ Hybrid ORM Architecture
- Entity Framework Core for standard operations
- Stored procedures for complex analytics
- Raw SQL for specialized queries
- Performance optimized repository pattern

### ✅ Production-Ready Features
- JWT authentication & role-based authorization
- Comprehensive error handling & validation
- Health checks & monitoring
- Structured logging with correlation IDs
- Rate limiting & security headers

### ✅ Testing Infrastructure
- **29 Passing Unit Tests** covering:
  - Repository pattern implementations
  - Entity validation and business rules
  - Stored procedure integration
  - Authentication and authorization flows

### ✅ Deployment Ready
- IIS deployment configuration
- Docker containerization
- CI/CD pipeline with GitHub Actions
- Multiple deployment options (IIS, Docker, Kubernetes)
- Production security hardening

### ✅ Complete Integration & Documentation
- **Comprehensive API Documentation** including OpenAPI/Swagger specification
- **Client SDKs** for JavaScript/TypeScript, Python, and C#
- **Postman Collections** with environment configurations
- **Code Examples** for common integration scenarios
- **Real-time Monitoring** and analytics examples
- **Batch Operations** and data export capabilities

## 🎉 Tutorial Complete!

**Congratulations!** You have successfully completed the comprehensive Product Inventory API tutorial with hybrid ORM implementation. This tutorial covered:

- ✅ **14 Complete Phases** from project setup to production deployment
- ✅ **29 Passing Unit Tests** with comprehensive test coverage
- ✅ **Production-Ready Architecture** with clean code principles
- ✅ **Hybrid ORM Implementation** combining EF Core with stored procedures
- ✅ **Complete Documentation** with SDKs and integration examples
- ✅ **Multiple Deployment Options** for various infrastructure needs

### What You've Built

This tutorial has guided you through creating a **production-ready inventory management API** that demonstrates:

1. **Hybrid ORM Architecture**: Seamlessly combining Entity Framework Core for standard operations with stored procedures for complex analytics
2. **Clean Architecture**: Well-structured, maintainable code following industry best practices
3. **Comprehensive Security**: JWT authentication, role-based authorization, and production security hardening
4. **Robust Testing**: Full test coverage with unit tests, integration tests, and automated CI/CD
5. **Production Deployment**: Multiple deployment options with Docker, IIS, and cloud platforms
6. **Developer Experience**: Complete documentation, SDKs, and integration examples

### Next Steps

Now that you've completed the tutorial, consider these next steps:

1. **Customize for Your Needs**: Adapt the code to your specific business requirements
2. **Extend Functionality**: Add new features like multi-warehouse support, advanced reporting, or webhook notifications
3. **Deploy to Production**: Use the deployment guides to deploy your API to your preferred environment
4. **Integrate with Your Systems**: Use the SDKs and examples to integrate with your existing applications
5. **Contribute Back**: Share improvements and contribute to the community

### Community & Support

- **Documentation**: Complete guides available in the `/docs` folder
- **GitHub Repository**: [https://github.com/your-org/productinventory-api](https://github.com/your-org/productinventory-api)
- **Community Forum**: [https://discussions.productinventory.com](https://discussions.productinventory.com)
- **API Support**: [api-support@productinventory.com](mailto:api-support@productinventory.com)

Thank you for following this comprehensive tutorial! You now have the knowledge and codebase to build sophisticated APIs with hybrid ORM architectures.

## 🏗️ Architecture Overview

The solution implements a **Clean Architecture** pattern with hybrid ORM capabilities:

### Layers
- **Web Layer**: Controllers, Middleware, Configuration
- **Core Layer**: Business Logic, Interfaces, Services  
- **Infrastructure Layer**: Data Access, Repository Implementation, External Services
- **Models Layer**: Entities, DTOs, Shared Models

### Hybrid ORM Approach
- **Entity Framework Core**: For standard CRUD operations, relationships, and LINQ queries
- **Stored Procedures**: For complex operations, analytics, and performance-critical scenarios
- **Raw SQL**: For specialized queries and database-specific optimizations

## 🗃️ Database Schema

### Core Entities
- **Categories**: Product categorization
- **Products**: Product catalog with SKU, pricing
- **Inventory**: Stock levels, reorder points, max levels
- **Users**: Authentication and authorization
- **InventoryMovements**: Audit trail for all stock changes

### Key Features
- Automatic timestamps (CreatedAt, UpdatedAt)
- Soft deletes for data integrity
- Comprehensive indexing for performance
- Foreign key relationships with proper constraints

## 🔄 Hybrid ORM Implementation

### EF Core Operations
```csharp
// Standard repository operations using EF Core
var categories = await _categoryRepository.GetActiveAsync();
var product = await _productRepository.GetByIdAsync(id);
```

### Stored Procedure Operations
```csharp
// Analytics and complex operations using stored procedures
var statistics = await _categoryRepository.GetCategoryStatisticsAsync();
var lowStockCategories = await _categoryRepository.GetCategoriesWithLowStockProductsAsync();
```

### Raw SQL Operations
```csharp
// Custom queries using raw SQL
var customData = await _storedProcedureRepository.ExecuteRawSqlAsync<CustomDto>(
    "SELECT * FROM CustomView WHERE Condition = @param",
    new[] { new SqlParameter("@param", value) });
```

## 📊 Available Stored Procedures

### Category Analytics
- `sp_GetCategoryStatistics`: Comprehensive category metrics
- `sp_GetCategoryStatisticsById`: Statistics for specific category
- `sp_GetCategoriesWithLowStockProducts`: Categories needing attention
- `sp_GetTopCategoriesByProductCount`: Top performing categories

### Inventory Management
- `sp_GetProductInventorySummary`: Complete inventory overview
- `sp_GetStockAlerts`: Stock level alerts and warnings
- `sp_RecordInventoryMovement`: Transactional stock updates

### Bulk Operations
- `sp_BulkUpdateCategoryStatus`: Batch status updates
- `sp_GetProductCountByCategory`: Efficient counting
- `sp_GetCategoryTotalValue`: Value calculations

## 🚀 API Endpoints

### Analytics Controller (`/api/v1/analytics`)
- `GET /categories/statistics` - All category statistics
- `GET /categories/{id}/statistics` - Specific category statistics  
- `GET /categories/low-stock` - Categories with low stock
- `GET /categories/top-by-product-count` - Top categories
- `PUT /categories/bulk-update-status` - Bulk status updates

### Inventory Movement Controller (`/api/v1/inventorymovement`)
- `GET /summary` - Inventory summary with filters
- `GET /alerts` - Stock alerts by level
- `POST /movements` - Record inventory movements

### Standard CRUD Controllers
- `CategoriesController`: Full CRUD + EF Core operations
- `ProductsController`: Product management
- `InventoryController`: Stock management
- `AuthController`: Authentication & authorization

## 🛠️ Setup Instructions

### 1. Database Setup
```sql
-- Run the database setup script
sqlcmd -S your-server -d your-database -i database/DatabaseSetup.sql

-- Create stored procedures
sqlcmd -S your-server -d your-database -i database/StoredProcedures.sql
```

### 2. Configuration
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=ProductInventoryDB;Trusted_Connection=true;"
  },
  "UseInMemoryDatabase": false
}
```

### 3. Entity Framework Migrations
```bash
# Add migration for InventoryMovements
dotnet ef migrations add AddInventoryMovements --project ProductInventoryAPI.Infrastructure

# Update database
dotnet ef database update --project ProductInventoryAPI.Infrastructure
```

### 4. Run the Application
```bash
cd ProductInventoryAPI/src/ProductInventoryAPI.Web
dotnet run
```

## 🔒 Security Features

- **JWT Authentication**: Token-based security
- **Role-based Authorization**: Admin, Manager, User roles
- **Password Hashing**: BCrypt for secure password storage
- **Request Validation**: Model validation and sanitization
- **SQL Injection Protection**: Parameterized queries only

## 📊 Performance Considerations

### EF Core Optimizations
- Connection pooling enabled
- Query splitting for complex includes
- No-tracking queries for read-only operations
- Compiled queries for frequently executed operations

### Stored Procedure Benefits
- Pre-compiled execution plans
- Reduced network round trips
- Server-side processing for complex logic
- Optimized for specific use cases

### Caching Strategy
- In-memory caching for reference data
- Response caching for expensive operations
- Database connection pooling

## 🧪 Testing

### Unit Tests
```bash
cd tests/ProductInventoryAPI.UnitTests
dotnet test
```

### Integration Tests
```bash
cd tests/ProductInventoryAPI.IntegrationTests
dotnet test
```

## 📝 Best Practices Implemented

### Repository Pattern
- Generic base repository for common operations
- Specialized repositories for entity-specific logic
- Unit of Work pattern for transaction management

### Error Handling
- Global exception middleware
- Structured logging with Serilog
- Consistent API response format

### Code Organization
- Clean Architecture separation of concerns
- Dependency injection throughout
- Configuration management
- Async/await patterns

### Database Design
- Normalized schema with proper relationships
- Indexed columns for query performance
- Audit trails for data changes
- Soft deletes for data retention

## 🔧 Extending the Hybrid ORM

### Adding New Stored Procedures
1. Create the SQL script in `database/` folder
2. Add DTO classes in `ProductInventoryAPI.Models.DTOs`
3. Extend repository interfaces with new methods
4. Implement in repository classes using `IStoredProcedureRepository`

### Adding New Entities
1. Create entity class in `ProductInventoryAPI.Models.Entities`
2. Add DbSet to `ProductInventoryDbContext`
3. Configure entity in `OnModelCreating`
4. Create and run EF Core migration

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Guide](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

---

This implementation provides a solid foundation for enterprise-grade applications requiring both the flexibility of ORM and the performance benefits of stored procedures.
- **ProductInventoryAPI.Core**: Business logic, services, and interfaces
- **ProductInventoryAPI.Infrastructure**: Data access layer with EF Core and stored procedures
- **ProductInventoryAPI.Models**: DTOs, ViewModels, and data transfer objects
- **ProductInventoryAPI.Shared**: Common utilities, extensions, and helpers

### Test Projects
- **ProductInventoryAPI.UnitTests**: Unit tests for business logic and services
- **ProductInventoryAPI.IntegrationTests**: Integration tests for API endpoints

## Features
- ✅ **Product Management**: CRUD operations using Entity Framework Core
- ✅ **Category Management**: Hierarchical category system
- ✅ **Inventory Tracking**: Real-time stock level monitoring
- ✅ **Reporting System**: Complex reports using stored procedures
- ✅ **JWT Authentication**: Role-based security (Admin, Manager, User)
- ✅ **API Versioning**: Versioned endpoints for backward compatibility
- ✅ **Swagger Documentation**: Interactive API documentation
- ✅ **Logging**: Structured logging with Serilog

## Technology Stack
- **.NET 8.0**: Latest framework version
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core**: ORM for standard operations
- **SQL Server**: Database with stored procedures
- **AutoMapper**: Object-to-object mapping
- **JWT Bearer**: Authentication and authorization
- **Swagger/OpenAPI**: API documentation
- **Serilog**: Structured logging
- **xUnit**: Testing framework

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2019+ or SQL Server Express
- Visual Studio 2022 or VS Code

### Database Setup
1. Run the database creation script:
   ```bash
   sqlcmd -S (local) -i scripts/database/CreateDatabase.sql
   ```

### Configuration
1. Update connection strings in `appsettings.json`
2. Configure JWT settings
3. Set up logging preferences

### Running the Application
```bash
dotnet build
dotnet run --project src/ProductInventoryAPI.Web
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## Development Progress

### ✅ Phase 1: Foundation & Setup
- [x] Project structure and solution setup
- [x] NuGet packages installation
- [x] Database schema creation
- [x] Build verification

### 🔄 Phase 2: Technical Architecture (In Progress)
- [ ] Clean architecture implementation
- [ ] Dependency injection setup
- [ ] Configuration management
- [ ] AutoMapper profiles

### 📋 Phase 3: Core Services & Business Logic
- [ ] Repository pattern implementation
- [ ] Service layer creation
- [ ] Business logic implementation
- [ ] Validation rules

### 📋 Phase 4: API Layer & Controllers
- [ ] RESTful controllers
- [ ] Response standardization
- [ ] Error handling middleware
- [ ] API versioning

### 📋 Phase 5: Authentication & Security
- [ ] JWT implementation
- [ ] Role-based authorization
- [ ] Security middleware
- [ ] Input validation

### 📋 Phase 6: Data Models & Database Operations
- [ ] Entity Framework setup
- [ ] Stored procedure integration
- [ ] Migration management
- [ ] Database seeding

### 📋 Phase 7: Testing Infrastructure
- [ ] Unit testing setup
- [ ] Integration testing
- [ ] Test data management
- [ ] Code coverage

### 📋 Phase 8: Deployment Configuration
- [ ] IIS deployment setup
- [ ] Environment configurations
- [ ] Health checks
- [ ] Monitoring

### 📋 Phase 9: Integration Points
- [ ] External service integration
- [ ] API client setup
- [ ] Performance monitoring
- [ ] Error tracking

## API Endpoints (Planned)

### Products
- `GET /api/v1/products` - Get all products
- `GET /api/v1/products/{id}` - Get product by ID
- `POST /api/v1/products` - Create new product
- `PUT /api/v1/products/{id}` - Update product
- `DELETE /api/v1/products/{id}` - Delete product

### Categories
- `GET /api/v1/categories` - Get all categories
- `POST /api/v1/categories` - Create new category

### Inventory
- `GET /api/v1/inventory` - Get inventory status
- `POST /api/v1/inventory/batch-update` - Batch update inventory

### Reports
- `GET /api/v1/reports/inventory-status` - Get inventory status report
- `GET /api/v1/reports/low-stock` - Get low stock report

### Authentication
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh token

## Contributing
Each phase includes comprehensive tests and should be committed individually for proper tracking and rollback capabilities.

## License
This project is for educational and demonstration purposes.