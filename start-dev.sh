#!/bin/bash

# Product Inventory API - Development Setup Script
# This script helps you choose between In-Memory database or SQL Server in Docker

echo "🚀 Product Inventory API - Development Setup"
echo "=============================================="
echo ""
echo "Choose your database option:"
echo "1. 🗄️  In-Memory Database (Quick start, no persistence)"
echo "2. 🐳 SQL Server in Docker (Full SQL Server, persistent)"
echo "3. ❌ Exit"
echo ""

read -p "Enter your choice (1-3): " choice

case $choice in
    1)
        echo "✅ Starting with In-Memory Database..."
        echo "📝 Updating configuration for In-Memory database..."
        
        # Update appsettings.Development.json to use in-memory database
        jq '.UseInMemoryDatabase = true' src/ProductInventoryAPI.Web/appsettings.Development.json > temp.json && mv temp.json src/ProductInventoryAPI.Web/appsettings.Development.json
        
        echo "🚀 Starting the API..."
        cd src/ProductInventoryAPI.Web
        dotnet run
        ;;
    2)
        echo "🐳 Starting SQL Server in Docker..."
        echo "📝 Updating configuration for SQL Server..."
        
        # Update appsettings.Development.json to use SQL Server
        jq '.UseInMemoryDatabase = false' src/ProductInventoryAPI.Web/appsettings.Development.json > temp.json && mv temp.json src/ProductInventoryAPI.Web/appsettings.Development.json
        
        # Start SQL Server container
        echo "🔄 Starting SQL Server container..."
        docker-compose up -d sqlserver
        
        # Wait for SQL Server to be ready
        echo "⏳ Waiting for SQL Server to be ready..."
        sleep 30
        
        # Check if SQL Server is ready
        echo "🔍 Checking SQL Server connection..."
        docker exec productinventory-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" > /dev/null 2>&1
        
        if [ $? -eq 0 ]; then
            echo "✅ SQL Server is ready!"
            echo "🚀 Starting the API..."
            cd src/ProductInventoryAPI.Web
            dotnet run
        else
            echo "❌ SQL Server failed to start properly"
            echo "🔄 Please check Docker logs: docker logs productinventory-db"
            exit 1
        fi
        ;;
    3)
        echo "👋 Goodbye!"
        exit 0
        ;;
    *)
        echo "❌ Invalid choice. Please run the script again."
        exit 1
        ;;
esac