-- =============================================
-- Product Inventory API - Stored Procedures
-- Phase 9: Stored Procedures for Hybrid ORM
-- =============================================

-- =============================================
-- Stored Procedure: sp_GetCategoryStatistics
-- Description: Get comprehensive category statistics including product counts, stock levels, and values
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetCategoryStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.Id AS CategoryId,
        c.Name AS CategoryName,
        COUNT(p.Id) AS ProductCount,
        ISNULL(SUM(i.CurrentStock), 0) AS TotalStock,
        ISNULL(SUM(i.CurrentStock * p.Price), 0) AS TotalValue,
        COUNT(CASE WHEN i.CurrentStock <= i.ReorderLevel THEN 1 END) AS LowStockProducts,
        MAX(ISNULL(i.UpdatedAt, c.UpdatedAt)) AS LastUpdated
    FROM Categories c
    LEFT JOIN Products p ON c.Id = p.CategoryId AND p.IsActive = 1
    LEFT JOIN Inventory i ON p.Id = i.ProductId
    WHERE c.IsActive = 1
    GROUP BY c.Id, c.Name
    ORDER BY c.Name;
END
GO

-- =============================================
-- Stored Procedure: sp_GetCategoryStatisticsById
-- Description: Get statistics for a specific category
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetCategoryStatisticsById]
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.Id AS CategoryId,
        c.Name AS CategoryName,
        COUNT(p.Id) AS ProductCount,
        ISNULL(SUM(i.CurrentStock), 0) AS TotalStock,
        ISNULL(SUM(i.CurrentStock * p.Price), 0) AS TotalValue,
        COUNT(CASE WHEN i.CurrentStock <= i.ReorderLevel THEN 1 END) AS LowStockProducts,
        MAX(ISNULL(i.UpdatedAt, c.UpdatedAt)) AS LastUpdated
    FROM Categories c
    LEFT JOIN Products p ON c.Id = p.CategoryId AND p.IsActive = 1
    LEFT JOIN Inventory i ON p.Id = i.ProductId
    WHERE c.IsActive = 1 AND c.Id = @CategoryId
    GROUP BY c.Id, c.Name;
END
GO

-- =============================================
-- Stored Procedure: sp_GetCategoriesWithLowStockProducts
-- Description: Get categories that have products with low stock
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetCategoriesWithLowStockProducts]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        c.Id,
        c.Name,
        c.Description,
        c.IsActive,
        c.CreatedAt,
        c.UpdatedAt
    FROM Categories c
    INNER JOIN Products p ON c.Id = p.CategoryId
    INNER JOIN Inventory i ON p.Id = i.ProductId
    WHERE c.IsActive = 1 
      AND p.IsActive = 1
      AND i.CurrentStock <= i.ReorderLevel
    ORDER BY c.Name;
END
GO

-- =============================================
-- Stored Procedure: sp_GetTopCategoriesByProductCount
-- Description: Get top N categories by product count
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetTopCategoriesByProductCount]
    @TopCount INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@TopCount)
        c.Id,
        c.Name,
        c.Description,
        c.IsActive,
        c.CreatedAt,
        c.UpdatedAt,
        COUNT(p.Id) AS ProductCount
    FROM Categories c
    LEFT JOIN Products p ON c.Id = p.CategoryId AND p.IsActive = 1
    WHERE c.IsActive = 1
    GROUP BY c.Id, c.Name, c.Description, c.IsActive, c.CreatedAt, c.UpdatedAt
    ORDER BY COUNT(p.Id) DESC, c.Name;
END
GO

-- =============================================
-- Stored Procedure: sp_BulkUpdateCategoryStatus
-- Description: Bulk update category active status
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_BulkUpdateCategoryStatus]
    @CategoryIds NVARCHAR(MAX),
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @UpdatedRows INT;
    
    -- Validate input
    IF @CategoryIds IS NULL OR LEN(TRIM(@CategoryIds)) = 0
    BEGIN
        RAISERROR('CategoryIds parameter is required', 16, 1);
        RETURN;
    END
    
    -- Create a table variable to hold the category IDs
    DECLARE @CategoryIdTable TABLE (CategoryId INT);
    
    -- Parse the comma-separated list of category IDs
    INSERT INTO @CategoryIdTable (CategoryId)
    SELECT CAST(value AS INT)
    FROM STRING_SPLIT(@CategoryIds, ',')
    WHERE ISNUMERIC(value) = 1;
    
    -- Update the categories
    UPDATE Categories 
    SET 
        IsActive = @IsActive,
        UpdatedAt = GETUTCDATE()
    WHERE Id IN (SELECT CategoryId FROM @CategoryIdTable);
    
    SET @UpdatedRows = @@ROWCOUNT;
    
    SELECT @UpdatedRows AS UpdatedRows;
END
GO

-- =============================================
-- Stored Procedure: sp_GetProductCountByCategory
-- Description: Get product count for a specific category
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetProductCountByCategory]
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS ProductCount
    FROM Products 
    WHERE CategoryId = @CategoryId AND IsActive = 1;
END
GO

-- =============================================
-- Stored Procedure: sp_GetCategoryTotalValue
-- Description: Get total value of products in a category
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetCategoryTotalValue]
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT ISNULL(SUM(i.CurrentStock * p.Price), 0) AS TotalValue
    FROM Products p
    INNER JOIN Inventory i ON p.Id = i.ProductId
    WHERE p.CategoryId = @CategoryId AND p.IsActive = 1;
END
GO

-- =============================================
-- Stored Procedure: sp_GetProductInventorySummary
-- Description: Get comprehensive product inventory summary
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetProductInventorySummary]
    @CategoryId INT = NULL,
    @LowStockOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.SKU AS ProductSku,
        c.Name AS CategoryName,
        i.CurrentStock,
        i.ReorderLevel,
        p.Price AS UnitPrice,
        (i.CurrentStock * p.Price) AS TotalValue,
        CASE WHEN i.CurrentStock <= i.ReorderLevel THEN 1 ELSE 0 END AS IsLowStock,
        i.UpdatedAt AS LastStockUpdate,
        CASE 
            WHEN i.CurrentStock = 0 THEN 'Out of Stock'
            WHEN i.CurrentStock <= i.ReorderLevel THEN 'Low Stock'
            WHEN i.CurrentStock > i.ReorderLevel * 2 THEN 'Overstocked'
            ELSE 'Normal'
        END AS Status
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.Id
    INNER JOIN Inventory i ON p.Id = i.ProductId
    WHERE p.IsActive = 1 
      AND c.IsActive = 1
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@LowStockOnly = 0 OR i.CurrentStock <= i.ReorderLevel)
    ORDER BY 
        c.Name, 
        p.Name;
END
GO

-- =============================================
-- Stored Procedure: sp_GetStockAlerts
-- Description: Get stock alert information for products
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetStockAlerts]
    @AlertLevel NVARCHAR(20) = 'All' -- 'Critical', 'Warning', 'Info', 'All'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.SKU AS ProductSku,
        c.Name AS CategoryName,
        i.CurrentStock,
        i.ReorderLevel,
        i.ReorderQuantity,
        CASE 
            WHEN i.CurrentStock = 0 THEN 'Critical'
            WHEN i.CurrentStock <= (i.ReorderLevel * 0.5) THEN 'Critical'
            WHEN i.CurrentStock <= i.ReorderLevel THEN 'Warning'
            ELSE 'Info'
        END AS AlertLevel,
        ISNULL(MAX(im.MovementDate), i.CreatedAt) AS LastRestocked,
        CASE 
            WHEN i.CurrentStock = 0 THEN DATEDIFF(DAY, ISNULL(MAX(im.MovementDate), i.CreatedAt), GETUTCDATE())
            ELSE 0
        END AS DaysOutOfStock
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.Id
    INNER JOIN Inventory i ON p.Id = i.ProductId
    LEFT JOIN InventoryMovements im ON p.Id = im.ProductId 
        AND im.MovementType IN ('Inbound', 'Purchase', 'Return')
        AND im.Quantity > 0
    WHERE p.IsActive = 1 
      AND c.IsActive = 1
      AND (
          @AlertLevel = 'All' OR
          (@AlertLevel = 'Critical' AND (i.CurrentStock = 0 OR i.CurrentStock <= (i.ReorderLevel * 0.5))) OR
          (@AlertLevel = 'Warning' AND i.CurrentStock <= i.ReorderLevel AND i.CurrentStock > (i.ReorderLevel * 0.5)) OR
          (@AlertLevel = 'Info' AND i.CurrentStock > i.ReorderLevel)
      )
    GROUP BY 
        p.Id, p.Name, p.SKU, c.Name, i.CurrentStock, i.ReorderLevel, 
        i.ReorderQuantity, i.CreatedAt
    ORDER BY 
        CASE 
            WHEN i.CurrentStock = 0 THEN 1
            WHEN i.CurrentStock <= (i.ReorderLevel * 0.5) THEN 2
            WHEN i.CurrentStock <= i.ReorderLevel THEN 3
            ELSE 4
        END,
        c.Name, 
        p.Name;
END
GO

-- =============================================
-- Stored Procedure: sp_RecordInventoryMovement
-- Description: Record an inventory movement and update current stock
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_RecordInventoryMovement]
    @ProductId INT,
    @MovementType NVARCHAR(20), -- 'Inbound', 'Outbound', 'Adjustment', 'Transfer', 'Sale', 'Purchase', 'Return'
    @Quantity INT,
    @Reason NVARCHAR(500) = NULL,
    @UserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentStock INT;
    DECLARE @NewStock INT;
    DECLARE @MovementId INT;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Get current stock
        SELECT @CurrentStock = CurrentStock 
        FROM Inventory 
        WHERE ProductId = @ProductId;
        
        IF @CurrentStock IS NULL
        BEGIN
            RAISERROR('Product not found in inventory', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Calculate new stock based on movement type
        SET @NewStock = CASE 
            WHEN @MovementType IN ('Inbound', 'Purchase', 'Return', 'Adjustment') AND @Quantity > 0 THEN @CurrentStock + @Quantity
            WHEN @MovementType IN ('Outbound', 'Sale', 'Transfer', 'Adjustment') AND @Quantity < 0 THEN @CurrentStock + @Quantity
            WHEN @MovementType IN ('Outbound', 'Sale', 'Transfer') AND @Quantity > 0 THEN @CurrentStock - @Quantity
            ELSE @CurrentStock
        END;
        
        -- Ensure stock doesn't go negative
        IF @NewStock < 0
        BEGIN
            RAISERROR('Insufficient stock. Current stock: %d, Requested: %d', 16, 1, @CurrentStock, @Quantity);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Record the movement
        INSERT INTO InventoryMovements (ProductId, MovementType, Quantity, PreviousStock, NewStock, Reason, MovementDate, CreatedBy)
        VALUES (@ProductId, @MovementType, @Quantity, @CurrentStock, @NewStock, @Reason, GETUTCDATE(), @UserId);
        
        SET @MovementId = SCOPE_IDENTITY();
        
        -- Update current stock
        UPDATE Inventory 
        SET 
            CurrentStock = @NewStock,
            UpdatedAt = GETUTCDATE()
        WHERE ProductId = @ProductId;
        
        COMMIT TRANSACTION;
        
        -- Return the movement details
        SELECT 
            @MovementId AS MovementId,
            @CurrentStock AS PreviousStock,
            @NewStock AS NewStock,
            'Movement recorded successfully' AS Message;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- Grant permissions (adjust as needed for your security model)
-- =============================================
-- GRANT EXECUTE ON [dbo].[sp_GetCategoryStatistics] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_GetCategoryStatisticsById] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_GetCategoriesWithLowStockProducts] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_GetTopCategoriesByProductCount] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_BulkUpdateCategoryStatus] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_GetProductCountByCategory] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_GetCategoryTotalValue] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_GetProductInventorySummary] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_GetStockAlerts] TO [YourAPIUser];
-- GRANT EXECUTE ON [dbo].[sp_RecordInventoryMovement] TO [YourAPIUser];

PRINT 'All stored procedures created successfully!';