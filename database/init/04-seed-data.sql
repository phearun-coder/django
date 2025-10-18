-- Seed data for Product Inventory System
USE ProductInventoryDB;
GO

-- Insert Categories
IF NOT EXISTS (SELECT * FROM Categories WHERE Name = 'Electronics')
BEGIN
    INSERT INTO Categories (Name, Description, IsActive) VALUES
    ('Electronics', 'Electronic devices and gadgets', 1),
    ('Clothing', 'Apparel and fashion items', 1),
    ('Books', 'Books and educational materials', 1),
    ('Home & Garden', 'Home improvement and gardening supplies', 1),
    ('Sports', 'Sports and fitness equipment', 1);
    
    PRINT 'Categories seeded successfully';
END
GO

-- Insert Users
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, Role, IsActive) VALUES
    ('admin', 'admin@productinventory.com', '$2a$11$rQZf9CQrGNZzJJ5cZ3ZjXOdKhJ6Yf8YqKGn5yA9Zq7Qv5KdMfGhIe', 'Admin', 1),
    ('manager', 'manager@productinventory.com', '$2a$11$rQZf9CQrGNZzJJ5cZ3ZjXOdKhJ6Yf8YqKGn5yA9Zq7Qv5KdMfGhIe', 'Manager', 1),
    ('user', 'user@productinventory.com', '$2a$11$rQZf9CQrGNZzJJ5cZ3ZjXOdKhJ6Yf8YqKGn5yA9Zq7Qv5KdMfGhIe', 'User', 1);
    
    PRINT 'Users seeded successfully (default password: Password123!)';
END
GO

-- Insert Products
IF NOT EXISTS (SELECT * FROM Products WHERE SKU = 'ELEC001')
BEGIN
    DECLARE @ElectronicsId int = (SELECT Id FROM Categories WHERE Name = 'Electronics');
    DECLARE @ClothingId int = (SELECT Id FROM Categories WHERE Name = 'Clothing');
    DECLARE @BooksId int = (SELECT Id FROM Categories WHERE Name = 'Books');
    DECLARE @HomeGardenId int = (SELECT Id FROM Categories WHERE Name = 'Home & Garden');
    DECLARE @SportsId int = (SELECT Id FROM Categories WHERE Name = 'Sports');
    
    INSERT INTO Products (Name, Description, SKU, Price, CategoryId, IsActive) VALUES
    -- Electronics
    ('Laptop Computer', 'High-performance laptop for business and gaming', 'ELEC001', 999.99, @ElectronicsId, 1),
    ('Wireless Mouse', 'Ergonomic wireless mouse with precision tracking', 'ELEC002', 29.99, @ElectronicsId, 1),
    ('Bluetooth Headphones', 'Noise-cancelling wireless headphones', 'ELEC003', 199.99, @ElectronicsId, 1),
    ('Smartphone', 'Latest model smartphone with advanced features', 'ELEC004', 799.99, @ElectronicsId, 1),
    ('Tablet', '10-inch tablet for productivity and entertainment', 'ELEC005', 399.99, @ElectronicsId, 1),
    
    -- Clothing
    ('T-Shirt', 'Comfortable cotton t-shirt', 'CLOT001', 19.99, @ClothingId, 1),
    ('Jeans', 'Classic blue denim jeans', 'CLOT002', 49.99, @ClothingId, 1),
    ('Sneakers', 'Comfortable running sneakers', 'CLOT003', 89.99, @ClothingId, 1),
    ('Jacket', 'Waterproof outdoor jacket', 'CLOT004', 129.99, @ClothingId, 1),
    ('Dress Shirt', 'Formal dress shirt for business', 'CLOT005', 39.99, @ClothingId, 1),
    
    -- Books
    ('Programming Guide', 'Complete guide to modern programming', 'BOOK001', 39.99, @BooksId, 1),
    ('Business Strategy', 'Strategic planning for business success', 'BOOK002', 29.99, @BooksId, 1),
    ('Science Textbook', 'Comprehensive science education book', 'BOOK003', 79.99, @BooksId, 1),
    ('Fiction Novel', 'Bestselling fiction novel', 'BOOK004', 14.99, @BooksId, 1),
    ('Cookbook', 'Healthy recipes for everyday cooking', 'BOOK005', 24.99, @BooksId, 1),
    
    -- Home & Garden
    ('Garden Tools Set', 'Complete set of essential garden tools', 'HOME001', 59.99, @HomeGardenId, 1),
    ('Kitchen Appliance', 'Multi-function kitchen appliance', 'HOME002', 149.99, @HomeGardenId, 1),
    ('Furniture Set', 'Modern furniture set for living room', 'HOME003', 899.99, @HomeGardenId, 1),
    ('Home Decor', 'Decorative items for home styling', 'HOME004', 34.99, @HomeGardenId, 1),
    ('Cleaning Supplies', 'Eco-friendly cleaning supplies kit', 'HOME005', 19.99, @HomeGardenId, 1),
    
    -- Sports
    ('Exercise Equipment', 'Home exercise equipment set', 'SPRT001', 299.99, @SportsId, 1),
    ('Soccer Ball', 'Professional grade soccer ball', 'SPRT002', 24.99, @SportsId, 1),
    ('Basketball', 'Official size basketball', 'SPRT003', 19.99, @SportsId, 1),
    ('Tennis Racket', 'Lightweight carbon fiber tennis racket', 'SPRT004', 129.99, @SportsId, 1),
    ('Yoga Mat', 'Non-slip yoga mat for exercise', 'SPRT005', 34.99, @SportsId, 1);
    
    PRINT 'Products seeded successfully';
END
GO

-- Insert Inventory records
IF NOT EXISTS (SELECT * FROM Inventory WHERE ProductId = 1)
BEGIN
    DECLARE @ProductCount int = (SELECT COUNT(*) FROM Products);
    DECLARE @Counter int = 1;
    
    WHILE @Counter <= @ProductCount
    BEGIN
        INSERT INTO Inventory (ProductId, Quantity, ReorderLevel, MaxStockLevel, LastRestockDate)
        VALUES (
            @Counter,
            CASE 
                WHEN @Counter <= 5 THEN 50 + (@Counter * 10)  -- Electronics: 60-100
                WHEN @Counter <= 10 THEN 100 + (@Counter * 5) -- Clothing: 105-150
                WHEN @Counter <= 15 THEN 30 + (@Counter * 3)  -- Books: 33-75
                WHEN @Counter <= 20 THEN 20 + (@Counter * 2)  -- Home & Garden: 22-60
                ELSE 40 + (@Counter * 2)                      -- Sports: 42-90
            END,
            CASE 
                WHEN @Counter <= 5 THEN 10   -- Electronics reorder level
                WHEN @Counter <= 10 THEN 20  -- Clothing reorder level
                WHEN @Counter <= 15 THEN 5   -- Books reorder level
                WHEN @Counter <= 20 THEN 15  -- Home & Garden reorder level
                ELSE 10                       -- Sports reorder level
            END,
            CASE 
                WHEN @Counter <= 5 THEN 200  -- Electronics max stock
                WHEN @Counter <= 10 THEN 300 -- Clothing max stock
                WHEN @Counter <= 15 THEN 100 -- Books max stock
                WHEN @Counter <= 20 THEN 150 -- Home & Garden max stock
                ELSE 200                      -- Sports max stock
            END,
            DATEADD(day, -RAND() * 30, GETUTCDATE()) -- Random last restock within 30 days
        );
        
        SET @Counter = @Counter + 1;
    END
    
    PRINT 'Inventory records seeded successfully';
END
GO

-- Insert some sample inventory movements
IF NOT EXISTS (SELECT * FROM InventoryMovements WHERE ProductId = 1)
BEGIN
    DECLARE @AdminUserId int = (SELECT Id FROM Users WHERE Username = 'admin');
    DECLARE @ManagerUserId int = (SELECT Id FROM Users WHERE Username = 'manager');
    
    INSERT INTO InventoryMovements (ProductId, MovementType, Quantity, Reason, CreatedBy, CreatedAt) VALUES
    (1, 'IN', 50, 'Initial stock', @AdminUserId, DATEADD(day, -30, GETUTCDATE())),
    (1, 'OUT', 5, 'Sales', @ManagerUserId, DATEADD(day, -25, GETUTCDATE())),
    (2, 'IN', 100, 'Bulk purchase', @AdminUserId, DATEADD(day, -28, GETUTCDATE())),
    (2, 'OUT', 15, 'Sales', @ManagerUserId, DATEADD(day, -20, GETUTCDATE())),
    (3, 'IN', 30, 'New shipment', @AdminUserId, DATEADD(day, -15, GETUTCDATE())),
    (3, 'OUT', 3, 'Customer orders', @ManagerUserId, DATEADD(day, -10, GETUTCDATE())),
    (4, 'IN', 25, 'Supplier delivery', @AdminUserId, DATEADD(day, -12, GETUTCDATE())),
    (5, 'IN', 40, 'Restock order', @AdminUserId, DATEADD(day, -8, GETUTCDATE())),
    (5, 'OUT', 8, 'Retail sales', @ManagerUserId, DATEADD(day, -5, GETUTCDATE())),
    (6, 'IN', 75, 'Seasonal stock', @AdminUserId, DATEADD(day, -7, GETUTCDATE()));
    
    PRINT 'Sample inventory movements seeded successfully';
END
GO

PRINT 'Database seeding completed successfully!';