using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Models.Entities;

namespace ProductInventoryAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Basic implementation of User repository (to be implemented with EF Core in later phases)
    /// </summary>
    public class UserRepository : IUserRepository
    {
        public Task<User> CreateAsync(User user)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<bool> ExistsAsync(int id)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<User> GetByEmailAsync(string email)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<User> GetByIdAsync(int id)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<User> UpdateAsync(User user)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<User> GetByRefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }

        public Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            throw new NotImplementedException("Repository will be implemented in Phase 7: Data Models & Database Operations");
        }
    }
}