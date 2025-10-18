# Product Inventory API - Deployment Guide

## Overview

This document provides comprehensive instructions for deploying the Product Inventory API to various environments including IIS, Docker, and cloud platforms.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [IIS Deployment](#iis-deployment)
3. [Docker Deployment](#docker-deployment)
4. [Configuration Management](#configuration-management)
5. [Database Setup](#database-setup)
6. [Security Considerations](#security-considerations)
7. [Monitoring and Logging](#monitoring-and-logging)
8. [Troubleshooting](#troubleshooting)

## Prerequisites

### Software Requirements

- **.NET 8.0 Runtime** (for IIS deployment)
- **.NET 8.0 SDK** (for building from source)
- **SQL Server 2019+** or **SQL Server Express**
- **IIS 10.0+** with ASP.NET Core Module (for IIS deployment)
- **Docker** and **Docker Compose** (for containerized deployment)

### Hardware Requirements

#### Minimum Requirements
- **CPU**: 2 cores
- **RAM**: 4 GB
- **Storage**: 10 GB free space
- **Network**: 100 Mbps

#### Recommended Requirements
- **CPU**: 4+ cores
- **RAM**: 8+ GB
- **Storage**: 50+ GB SSD
- **Network**: 1 Gbps

## IIS Deployment

### 1. Prepare the Server

```powershell
# Install IIS and ASP.NET Core Module
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Security
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering

# Download and install ASP.NET Core Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 2. Deploy Using PowerShell Script

```powershell
# Run the deployment script
.\deployment\Deploy-ProductInventoryAPI.ps1 `
    -TargetServer "your-server.com" `
    -SiteName "ProductInventoryAPI" `
    -ApplicationPool "ProductInventoryAPI" `
    -DeploymentPath "C:\inetpub\wwwroot\ProductInventoryAPI" `
    -Configuration "Release" `
    -RunTests
```

### 3. Manual Deployment Steps

If you prefer manual deployment:

1. **Build and Publish**:
   ```bash
   dotnet publish src/ProductInventoryAPI.Web/ -c Release -o ./publish
   ```

2. **Copy Files**:
   - Copy published files to `C:\inetpub\wwwroot\ProductInventoryAPI`
   - Copy `deployment/web.config` to the deployment directory
   - Copy `deployment/appsettings.Production.json` to the deployment directory

3. **Configure IIS**:
   - Create Application Pool "ProductInventoryAPI"
   - Set .NET CLR Version to "No Managed Code"
   - Create Website pointing to deployment directory
   - Set Application Pool to "ProductInventoryAPI"

## Docker Deployment

### 1. Using Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/your-repo/ProductInventoryAPI.git
cd ProductInventoryAPI

# Build and start services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f api
```

### 2. Manual Docker Deployment

```bash
# Build the Docker image
docker build -t productinventory-api .

# Run SQL Server container
docker run -d \
  --name productinventory-db \
  -e ACCEPT_EULA=Y \
  -e SA_PASSWORD=YourStrong@Passw0rd \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest

# Run API container
docker run -d \
  --name productinventory-api \
  --link productinventory-db:sqlserver \
  -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="Server=sqlserver,1433;Database=ProductInventoryDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true" \
  productinventory-api
```

### 3. Production Docker Deployment

For production, use the provided `docker-compose.yml` with modifications:

1. **Update Environment Variables**:
   ```yaml
   environment:
     - ConnectionStrings__DefaultConnection=your-production-connection-string
     - JwtSettings__SecretKey=your-production-secret-key
   ```

2. **Add SSL Certificates**:
   ```yaml
   volumes:
     - ./ssl:/etc/nginx/ssl
   ```

3. **Configure External Database**:
   Remove the `sqlserver` service and point to your production database.

## Configuration Management

### Environment-Specific Configuration

#### Development
- Use `appsettings.Development.json`
- Enable Swagger documentation
- Use local SQL Server or SQL Server Express
- Detailed logging enabled

#### Staging
- Use `appsettings.Staging.json`
- Limited Swagger access
- Use staging database
- Moderate logging

#### Production
- Use `appsettings.Production.json`
- Disable Swagger
- Use production database with connection pooling
- Error-level logging only

### Configuration Files

#### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-production-connection-string"
  },
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-key",
    "Issuer": "ProductInventoryAPI",
    "Audience": "ProductInventoryAPI.Users",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables

For containerized deployments, use environment variables:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="your-connection-string"
export JwtSettings__SecretKey="your-secret-key"
```

## Database Setup

### 1. Automated Setup (Docker)

The Docker Compose configuration automatically:
- Creates SQL Server container
- Runs initialization scripts
- Creates database schema
- Seeds initial data

### 2. Manual Database Setup

```sql
-- 1. Create database
sqlcmd -S your-server -U sa -P your-password -i database/init/01-create-database.sql

-- 2. Create tables
sqlcmd -S your-server -U sa -P your-password -d ProductInventoryDB -i database/init/02-create-tables.sql

-- 3. Create stored procedures
sqlcmd -S your-server -U sa -P your-password -d ProductInventoryDB -i database/init/03-create-stored-procedures.sql

-- 4. Seed initial data
sqlcmd -S your-server -U sa -P your-password -d ProductInventoryDB -i database/init/04-seed-data.sql
```

### 3. Entity Framework Migrations

For development and staging environments:

```bash
# Add migration
dotnet ef migrations add InitialCreate --project src/ProductInventoryAPI.Infrastructure

# Update database
dotnet ef database update --project src/ProductInventoryAPI.Infrastructure
```

## Security Considerations

### 1. SSL/TLS Configuration

#### IIS Configuration
- Enable HTTPS binding
- Install valid SSL certificate
- Configure HSTS headers
- Redirect HTTP to HTTPS

#### Docker/Nginx Configuration
- Use Let's Encrypt for SSL certificates
- Configure SSL termination at proxy level
- Set security headers

### 2. Authentication and Authorization

- **JWT Token Security**: Use strong secret keys (256-bit minimum)
- **Password Policies**: Enforce strong password requirements
- **Rate Limiting**: Implement API rate limiting
- **CORS Configuration**: Restrict origins in production

### 3. Database Security

- **Connection Security**: Use encrypted connections
- **SQL Injection Prevention**: Parameterized queries only
- **Database Permissions**: Principle of least privilege
- **Regular Backups**: Automated backup strategy

### 4. Network Security

- **Firewall Configuration**: Restrict unnecessary ports
- **VPN Access**: Secure administrative access
- **API Gateway**: Use API gateway for external access
- **DDoS Protection**: Implement DDoS mitigation

## Monitoring and Logging

### 1. Application Logging

Configure structured logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ProductInventoryAPI": "Information"
    },
    "File": {
      "Path": "logs/app-.log",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 30
    }
  }
}
```

### 2. Health Checks

The API includes health check endpoints:
- `/health` - Basic health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### 3. Performance Monitoring

#### Application Insights (Azure)
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-app-insights-key"
  }
}
```

#### Prometheus Metrics
For containerized deployments, metrics are exposed at `/metrics`

### 4. Log Aggregation

#### ELK Stack
- Elasticsearch for storage
- Logstash for processing
- Kibana for visualization

#### Grafana + Loki
- Grafana for dashboards
- Loki for log aggregation
- Prometheus for metrics

## Troubleshooting

### Common Issues

#### 1. Application Won't Start

**Symptoms**: HTTP 500 errors, application crashes

**Solutions**:
- Check application logs in `logs/` directory
- Verify database connection string
- Ensure SQL Server is accessible
- Check IIS application pool identity permissions

#### 2. Database Connection Issues

**Symptoms**: "Cannot connect to database" errors

**Solutions**:
- Verify SQL Server is running
- Check network connectivity
- Validate connection string
- Verify SQL Server authentication mode
- Check firewall settings

#### 3. Authentication Problems

**Symptoms**: 401 Unauthorized errors

**Solutions**:
- Verify JWT secret key configuration
- Check token expiration
- Validate issuer and audience settings
- Ensure HTTPS for token transmission

#### 4. Performance Issues

**Symptoms**: Slow response times, timeouts

**Solutions**:
- Check database query performance
- Monitor CPU and memory usage
- Analyze application logs for bottlenecks
- Consider connection pooling
- Review caching strategy

### Diagnostic Commands

#### Check Application Status
```bash
# Docker
docker-compose ps
docker-compose logs api

# IIS
Get-Website -Name "ProductInventoryAPI"
Get-WebApplication -Site "ProductInventoryAPI"
```

#### Database Connectivity
```sql
-- Test database connection
SELECT @@VERSION;
SELECT COUNT(*) FROM Categories;
```

#### Log Analysis
```bash
# View recent logs
tail -f logs/app-*.log

# Search for errors
grep -i error logs/app-*.log
```

## Support

For deployment support and troubleshooting:

1. **Documentation**: Check this deployment guide
2. **Logs**: Review application and system logs
3. **Health Checks**: Monitor health check endpoints
4. **Performance**: Use monitoring tools for performance analysis

## Version History

- **v1.0.0**: Initial release with IIS and Docker deployment support
- **v1.1.0**: Added nginx reverse proxy configuration
- **v1.2.0**: Enhanced security and monitoring features