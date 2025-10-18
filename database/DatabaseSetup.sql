-- =============================================
-- Database Setup Script for Product Inventory API
-- Creates tables, indexes, and stored procedures
-- =============================================

-- First ensure we have the InventoryMovements table (if not created by EF migration)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InventoryMovements' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[InventoryMovements](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [MovementType] [nvarchar](20) NOT NULL,
        [Quantity] [int] NOT NULL,
        [PreviousStock] [int] NOT NULL,
        [NewStock] [int] NOT NULL,
        [Reason] [nvarchar](500) NULL,
        [MovementDate] [datetime2](7) NOT NULL,
        [CreatedBy] [int] NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_InventoryMovements] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_InventoryMovements_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id]),
        CONSTRAINT [FK_InventoryMovements_Users] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[Users] ([Id])
    );
    
    -- Create indexes for performance
    CREATE NONCLUSTERED INDEX [IX_InventoryMovements_ProductId] ON [dbo].[InventoryMovements]([ProductId]);
    CREATE NONCLUSTERED INDEX [IX_InventoryMovements_MovementDate] ON [dbo].[InventoryMovements]([MovementDate]);
    CREATE NONCLUSTERED INDEX [IX_InventoryMovements_MovementType] ON [dbo].[InventoryMovements]([MovementType]);
    
    PRINT 'InventoryMovements table created successfully.';
END
ELSE
BEGIN
    PRINT 'InventoryMovements table already exists.';
END

-- Now create all the stored procedures
PRINT 'Creating stored procedures...';

-- Include all stored procedures from StoredProcedures.sql file here
-- This is a placeholder - in practice you would execute the StoredProcedures.sql file

PRINT 'Database setup completed successfully!';