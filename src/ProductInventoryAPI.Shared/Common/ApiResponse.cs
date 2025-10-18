namespace ProductInventoryAPI.Shared.Common
{
    /// <summary>
    /// Standard API response wrapper for consistent response format
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string message = "Success")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        public ApiResponse(string message, IEnumerable<string>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors;
        }

        public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse<T> ErrorResult(string message, IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>(message, errors);
        }

        public static ApiResponse<T> ErrorResult(string message, string error)
        {
            return new ApiResponse<T>(message, new[] { error });
        }
    }

    /// <summary>
    /// API response for operations that don't return data
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse() : base() { }
        public ApiResponse(string message) : base(message) { }
        public ApiResponse(string message, IEnumerable<string>? errors) : base(message, errors) { }

        public static ApiResponse SuccessResult(string message = "Success")
        {
            return new ApiResponse { Success = true, Message = message };
        }

        public static new ApiResponse ErrorResult(string message, IEnumerable<string>? errors = null)
        {
            return new ApiResponse(message, errors);
        }

        public static new ApiResponse ErrorResult(string message, string error)
        {
            return new ApiResponse(message, new[] { error });
        }
    }
}