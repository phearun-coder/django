using Microsoft.EntityFrameworkCore;
using ProductInventoryAPI.Models.Entities;

namespace ProductInventoryAPI.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for Product Inventory API
/// </summary>
public class ProductInventoryDbContext : DbContext
{
    public ProductInventoryDbContext(DbContextOptions<ProductInventoryDbContext> options)
        : base(options)
    {
    }

    // DbSets for entities
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Foreign key relationship
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // One-to-one relationship with Inventory
            entity.HasOne(e => e.Inventory)
                  .WithOne(i => i.Product)
                  .HasForeignKey<Inventory>(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.Name, e.CategoryId });
        });

        // Configure Inventory entity
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuantityOnHand).HasDefaultValue(0);
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.ReorderLevel).HasDefaultValue(0);
            entity.Property(e => e.ReorderPoint).HasDefaultValue(0);
            entity.Property(e => e.MaxStockLevel).HasDefaultValue(100);
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.HasIndex(e => e.QuantityOnHand);
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("User");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure InventoryMovement entity
        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MovementType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.MovementDate).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.MovementDate);
            entity.HasIndex(e => e.MovementType);
            entity.HasIndex(e => e.CreatedBy);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = 3, Name = "Books", Description = "Books and educational materials", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = 4, Name = "Home & Garden", Description = "Home improvement and garden supplies", IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Smartphone", Description = "Latest model smartphone", SKU = "ELEC-SP-001", CategoryId = 1, Price = 599.99m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Laptop", Description = "High-performance laptop", SKU = "ELEC-LP-001", CategoryId = 1, Price = 1299.99m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "T-Shirt", Description = "Cotton t-shirt", SKU = "CLTH-TS-001", CategoryId = 2, Price = 24.99m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, Name = "Programming Guide", Description = "Comprehensive programming guide", SKU = "BOOK-PG-001", CategoryId = 3, Price = 49.99m, IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        // Seed Inventory
        modelBuilder.Entity<Inventory>().HasData(
            new Inventory { Id = 1, ProductId = 1, QuantityOnHand = 50, Quantity = 50, ReorderLevel = 10, ReorderPoint = 10, MaxStockLevel = 100, LastUpdated = DateTime.UtcNow },
            new Inventory { Id = 2, ProductId = 2, QuantityOnHand = 25, Quantity = 25, ReorderLevel = 5, ReorderPoint = 5, MaxStockLevel = 50, LastUpdated = DateTime.UtcNow },
            new Inventory { Id = 3, ProductId = 3, QuantityOnHand = 200, Quantity = 200, ReorderLevel = 50, ReorderPoint = 50, MaxStockLevel = 500, LastUpdated = DateTime.UtcNow },
            new Inventory { Id = 4, ProductId = 4, QuantityOnHand = 75, Quantity = 75, ReorderLevel = 15, ReorderPoint = 15, MaxStockLevel = 150, LastUpdated = DateTime.UtcNow }
        );

        // Seed Users (with default admin user)
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Username = "admin", 
                Email = "admin@productinventory.com", 
                FirstName = "System", 
                LastName = "Administrator", 
                PasswordHash = "$2a$11$mKGjfSNlWWWFjQgKwjwmjOv8bYDJY1Q8ZxnzKfPxHJxC8rQ7gWX3u", // BCrypt hash for "Admin123!"
                Role = "Admin", 
                IsActive = true, 
                CreatedAt = DateTime.UtcNow 
            },
            new User 
            { 
                Id = 2, 
                Username = "manager", 
                Email = "manager@productinventory.com", 
                FirstName = "Inventory", 
                LastName = "Manager", 
                PasswordHash = "$2a$11$mKGjfSNlWWWFjQgKwjwmjOv8bYDJY1Q8ZxnzKfPxHJxC8rQ7gWX3u", // BCrypt hash for "Manager123!"
                Role = "Manager", 
                IsActive = true, 
                CreatedAt = DateTime.UtcNow 
            }
        );
    }

    /// <summary>
    /// Override SaveChanges to automatically update timestamps
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update timestamps
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Category || e.Entity is Product || e.Entity is User)
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property("CreatedAt") != null)
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }

            if (entry.Property("UpdatedAt") != null)
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;

            // Special handling for Inventory
            if (entry.Entity is Inventory && entry.Property("LastUpdated") != null)
                entry.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
        }
    }
}