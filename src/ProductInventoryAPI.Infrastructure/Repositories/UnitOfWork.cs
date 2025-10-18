using Microsoft.EntityFrameworkCore.Storage;
using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Infrastructure.Data;

namespace ProductInventoryAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework implementation of Unit of Work pattern
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ProductInventoryDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        public ICategoryRepository Categories { get; }
        public IProductRepository Products { get; }
        public IInventoryRepository Inventory { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(
            ProductInventoryDbContext context,
            ICategoryRepository categories,
            IProductRepository products,
            IInventoryRepository inventory,
            IUserRepository users)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Categories = categories ?? throw new ArgumentNullException(nameof(categories));
            Products = products ?? throw new ArgumentNullException(nameof(products));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        /// <summary>
        /// Save all changes to the database
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                // Note: Don't dispose the context here as it's managed by DI container
                _disposed = true;
            }
        }
    }
}