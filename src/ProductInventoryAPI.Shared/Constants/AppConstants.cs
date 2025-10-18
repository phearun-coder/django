namespace ProductInventoryAPI.Shared.Constants
{
    /// <summary>
    /// Application constants
    /// </summary>
    public static class AppConstants
    {
        public const string ApiVersion = "v1";
        public const string ApiName = "Product Inventory API";
        
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Manager = "Manager";
            public const string User = "User";
        }

        /// <summary>
        /// User roles for authentication and authorization
        /// </summary>
        public static class UserRoles
        {
            public const string Admin = "Admin";
            public const string Manager = "Manager";
            public const string User = "User";

            public static readonly string[] AllRoles = { Admin, Manager, User };
        }

        public static class Policies
        {
            public const string AdminOnly = "AdminOnly";
            public const string ManagerOrAbove = "ManagerOrAbove";
            public const string AllUsers = "AllUsers";
        }

        public static class Pagination
        {
            public const int DefaultPageSize = 10;
            public const int MaxPageSize = 100;
        }

        public static class StockStatus
        {
            public const string Low = "Low";
            public const string Normal = "Normal";
            public const string High = "High";
            public const string OutOfStock = "OutOfStock";
        }

        public static class ValidationMessages
        {
            public const string Required = "This field is required";
            public const string InvalidEmail = "Invalid email format";
            public const string InvalidFormat = "Invalid format";
            public const string MinLength = "Minimum length is {0} characters";
            public const string MaxLength = "Maximum length is {0} characters";
            public const string Range = "Value must be between {0} and {1}";
        }
    }
}