-- Create Tables for Product Inventory System
USE ProductInventoryDB;
GO

-- Categories Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        Description nvarchar(500) NULL,
        IsActive bit NOT NULL DEFAULT 1,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_Categories_Name ON Categories(Name);
    CREATE INDEX IX_Categories_IsActive ON Categories(IsActive);
    
    PRINT 'Categories table created successfully';
END
GO

-- Products Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        Description nvarchar(1000) NULL,
        SKU nvarchar(50) NOT NULL UNIQUE,
        Price decimal(18,2) NOT NULL,
        CategoryId int NOT NULL,
        IsActive bit NOT NULL DEFAULT 1,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
    );
    
    CREATE INDEX IX_Products_Name ON Products(Name);
    CREATE INDEX IX_Products_SKU ON Products(SKU);
    CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
    CREATE INDEX IX_Products_IsActive ON Products(IsActive);
    
    PRINT 'Products table created successfully';
END
GO

-- Inventory Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventory')
BEGIN
    CREATE TABLE Inventory (
        Id int IDENTITY(1,1) PRIMARY KEY,
        ProductId int NOT NULL,
        Quantity int NOT NULL DEFAULT 0,
        ReorderLevel int NOT NULL DEFAULT 0,
        MaxStockLevel int NULL,
        LastRestockDate datetime2 NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Inventory_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
        CONSTRAINT CK_Inventory_Quantity CHECK (Quantity >= 0),
        CONSTRAINT CK_Inventory_ReorderLevel CHECK (ReorderLevel >= 0)
    );
    
    CREATE UNIQUE INDEX IX_Inventory_ProductId ON Inventory(ProductId);
    
    PRINT 'Inventory table created successfully';
END
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Username nvarchar(50) NOT NULL UNIQUE,
        Email nvarchar(256) NOT NULL UNIQUE,
        PasswordHash nvarchar(255) NOT NULL,
        Role nvarchar(50) NOT NULL DEFAULT 'User',
        IsActive bit NOT NULL DEFAULT 1,
        LastLoginAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_Users_Username ON Users(Username);
    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_Role ON Users(Role);
    CREATE INDEX IX_Users_IsActive ON Users(IsActive);
    
    PRINT 'Users table created successfully';
END
GO

-- InventoryMovements Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InventoryMovements')
BEGIN
    CREATE TABLE InventoryMovements (
        Id int IDENTITY(1,1) PRIMARY KEY,
        ProductId int NOT NULL,
        MovementType nvarchar(10) NOT NULL,
        Quantity int NOT NULL,
        Reason nvarchar(500) NOT NULL,
        CreatedBy int NOT NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_InventoryMovements_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
        CONSTRAINT FK_InventoryMovements_Users FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT CK_InventoryMovements_MovementType CHECK (MovementType IN ('IN', 'OUT', 'ADJUSTMENT')),
        CONSTRAINT CK_InventoryMovements_Quantity CHECK (Quantity > 0)
    );
    
    CREATE INDEX IX_InventoryMovements_ProductId ON InventoryMovements(ProductId);
    CREATE INDEX IX_InventoryMovements_MovementType ON InventoryMovements(MovementType);
    CREATE INDEX IX_InventoryMovements_CreatedAt ON InventoryMovements(CreatedAt);
    CREATE INDEX IX_InventoryMovements_CreatedBy ON InventoryMovements(CreatedBy);
    
    PRINT 'InventoryMovements table created successfully';
END
GO

PRINT 'All tables created successfully!';