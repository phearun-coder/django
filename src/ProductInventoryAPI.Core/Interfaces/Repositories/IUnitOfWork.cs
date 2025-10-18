namespace ProductInventoryAPI.Core.Interfaces.Repositories
{
    /// <summary>
    /// Unit of Work pattern interface for managing transactions across repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IInventoryRepository Inventory { get; }
        IUserRepository Users { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}