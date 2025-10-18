# Use the official ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/ProductInventoryAPI.Web/ProductInventoryAPI.Web.csproj", "src/ProductInventoryAPI.Web/"]
COPY ["src/ProductInventoryAPI.Core/ProductInventoryAPI.Core.csproj", "src/ProductInventoryAPI.Core/"]
COPY ["src/ProductInventoryAPI.Infrastructure/ProductInventoryAPI.Infrastructure.csproj", "src/ProductInventoryAPI.Infrastructure/"]
COPY ["src/ProductInventoryAPI.Models/ProductInventoryAPI.Models.csproj", "src/ProductInventoryAPI.Models/"]
COPY ["src/ProductInventoryAPI.Shared/ProductInventoryAPI.Shared.csproj", "src/ProductInventoryAPI.Shared/"]

# Restore NuGet packages
RUN dotnet restore "src/ProductInventoryAPI.Web/ProductInventoryAPI.Web.csproj"

# Copy the entire source code
COPY . .

# Build the application
WORKDIR "/src/src/ProductInventoryAPI.Web"
RUN dotnet build "ProductInventoryAPI.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ProductInventoryAPI.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Copy deployment configuration
COPY deployment/appsettings.Production.json ./appsettings.Production.json

# Create logs directory and set permissions
RUN mkdir -p /app/logs && \
    chown -R appuser:appuser /app && \
    chmod -R 755 /app

# Install curl for health checks
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Switch to non-root user
USER appuser

# Add health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "ProductInventoryAPI.Web.dll"]