# Product Inventory API - Development Configuration

## Quick Start (In-Memory Database)

For the fastest setup without external dependencies:

```bash
cd /workspaces/django/ProductInventoryAPI
./start-dev.sh
# Choose option 1 for In-Memory Database
```

Or manually:
```bash
cd src/ProductInventoryAPI.Web
export ASPNETCORE_ENVIRONMENT=Development
export UseInMemoryDatabase=true
dotnet run
```

## SQL Server in Docker

For full SQL Server functionality:

```bash
cd /workspaces/django/ProductInventoryAPI
./start-dev.sh
# Choose option 2 for SQL Server in Docker
```

Or manually:
```bash
# Start SQL Server
docker-compose up -d sqlserver

# Wait for it to be ready (30 seconds)
sleep 30

# Set environment and run
cd src/ProductInventoryAPI.Web
export ASPNETCORE_ENVIRONMENT=Development
export UseInMemoryDatabase=false
dotnet run
```

## Access Points

- **API**: http://localhost:5000 or https://localhost:5001
- **Swagger UI**: http://localhost:5000/swagger
- **SQL Server**: localhost:1433 (if using Docker)
  - Username: `sa`
  - Password: `YourStrong@Passw0rd`
- **Adminer (SQL Web UI)**: http://localhost:8080 (if using Docker)

## Configuration Details

### In-Memory Database
- ✅ No external dependencies
- ✅ Fast startup
- ✅ Perfect for development and testing
- ❌ Data is lost when application stops
- ❌ No stored procedures

### SQL Server in Docker
- ✅ Full SQL Server functionality
- ✅ Persistent data
- ✅ Stored procedures support
- ✅ Real database relationships
- ❌ Requires Docker
- ❌ Slower startup

## Environment Variables

You can also control the database choice using environment variables:

```bash
# Use In-Memory Database
export UseInMemoryDatabase=true

# Use SQL Server
export UseInMemoryDatabase=false
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=ProductInventoryDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true;"
```

## Troubleshooting

### LocalDB Error on Linux
If you see "LocalDB is not supported on this platform", it means the application is trying to use Windows-only LocalDB. Use either:
1. In-Memory database (set `UseInMemoryDatabase=true`)
2. SQL Server in Docker (run `docker-compose up -d sqlserver`)

### Connection Issues
- Ensure SQL Server container is running: `docker ps`
- Check container logs: `docker logs productinventory-db`
- Verify connection string matches the container configuration