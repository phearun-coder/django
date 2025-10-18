using ProductInventoryAPI.Core.Exceptions;
using ProductInventoryAPI.Shared.Common;
using System.Net;
using System.Text.Json;

namespace ProductInventoryAPI.Web.Middleware
{
    /// <summary>
    /// Global exception handling middleware for consistent error responses
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse<object>();

            switch (exception)
            {
                case NotFoundException ex:
                    response = ApiResponse<object>.ErrorResult(ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case ValidationException ex:
                    response = ApiResponse<object>.ErrorResult("Validation failed", ex.Errors);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case BusinessRuleViolationException ex:
                    response = ApiResponse<object>.ErrorResult(ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;

                case UnauthorizedAccessException:
                    response = ApiResponse<object>.ErrorResult("Unauthorized access");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                default:
                    response = ApiResponse<object>.ErrorResult("An error occurred while processing your request");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}