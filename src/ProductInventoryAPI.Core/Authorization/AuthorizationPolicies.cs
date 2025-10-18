namespace ProductInventoryAPI.Core.Authorization
{
    /// <summary>
    /// Authorization policy names and roles constants
    /// </summary>
    public static class AuthorizationPolicies
    {
        // Policy Names
        public const string AdminOnly = "AdminOnly";
        public const string ManagerOrAdmin = "ManagerOrAdmin";
        public const string AllRoles = "AllRoles";
        public const string InventoryAccess = "InventoryAccess";
        public const string CategoryManagement = "CategoryManagement";
        public const string ProductManagement = "ProductManagement";
        public const string UserManagement = "UserManagement";
        
        // Role Names (should match UserRoles in Shared constants)
        public const string AdminRole = "Admin";
        public const string ManagerRole = "Manager";
        public const string UserRole = "User";
        
        /// <summary>
        /// Get all available policies for registration
        /// </summary>
        public static Dictionary<string, string[]> GetPolicies()
        {
            return new Dictionary<string, string[]>
            {
                { AdminOnly, new[] { AdminRole } },
                { ManagerOrAdmin, new[] { ManagerRole, AdminRole } },
                { AllRoles, new[] { UserRole, ManagerRole, AdminRole } },
                { InventoryAccess, new[] { ManagerRole, AdminRole } },
                { CategoryManagement, new[] { ManagerRole, AdminRole } },
                { ProductManagement, new[] { ManagerRole, AdminRole } },
                { UserManagement, new[] { AdminRole } }
            };
        }
    }
}