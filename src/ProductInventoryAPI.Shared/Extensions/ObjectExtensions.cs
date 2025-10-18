using System.Text.Json;

namespace ProductInventoryAPI.Shared.Extensions
{
    /// <summary>
    /// Extension methods for objects
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts an object to JSON string
        /// </summary>
        public static string ToJson(this object obj, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize(obj, options ?? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        }

        /// <summary>
        /// Creates a deep copy of an object using JSON serialization
        /// </summary>
        public static T? DeepCopy<T>(this T obj) where T : class
        {
            if (obj == null) return null;
            
            var json = obj.ToJson();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}