-- Create Product Inventory Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ProductInventoryDB')
BEGIN
    CREATE DATABASE ProductInventoryDB;
    PRINT 'ProductInventoryDB database created successfully';
END
ELSE
BEGIN
    PRINT 'ProductInventoryDB database already exists';
END
GO

-- Set database properties
ALTER DATABASE ProductInventoryDB SET RECOVERY SIMPLE;
ALTER DATABASE ProductInventoryDB SET AUTO_CLOSE OFF;
ALTER DATABASE ProductInventoryDB SET AUTO_SHRINK OFF;
GO