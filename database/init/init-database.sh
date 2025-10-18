#!/bin/bash
# Wait for SQL Server to start
echo "Waiting for SQL Server to start..."
sleep 30s

# Create database and run initialization scripts
echo "Creating database and running initialization scripts..."

/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -i /docker-entrypoint-initdb.d/01-create-database.sql
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d ProductInventoryDB -i /docker-entrypoint-initdb.d/02-create-tables.sql
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d ProductInventoryDB -i /docker-entrypoint-initdb.d/03-create-stored-procedures.sql
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d ProductInventoryDB -i /docker-entrypoint-initdb.d/04-seed-data.sql

echo "Database initialization completed!"