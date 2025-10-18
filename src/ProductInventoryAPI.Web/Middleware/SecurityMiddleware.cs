using Microsoft.Extensions.Primitives;
using System.Net;

namespace ProductInventoryAPI.Web.Middleware
{
    /// <summary>
    /// Security middleware for additional security headers and basic rate limiting
    /// </summary>
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;
        private readonly Dictionary<string, DateTime> _requestCounts = new();
        private readonly object _lock = new();

        public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            AddSecurityHeaders(context);

            // Basic rate limiting (in production, use Redis or proper rate limiting middleware)
            if (!IsRateLimitAllowed(context))
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            // Log security relevant information
            LogSecurityInfo(context);

            await _next(context);
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;

            // Security headers
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            
            // Content Security Policy (adjust as needed)
            headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
            
            // HSTS (only for HTTPS)
            if (context.Request.IsHttps)
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            // API-specific headers
            headers["X-API-Version"] = "1.0";
            headers["X-Request-ID"] = Guid.NewGuid().ToString();
        }

        private bool IsRateLimitAllowed(HttpContext context)
        {
            // Simple rate limiting - 100 requests per minute per IP
            // In production, use a proper rate limiting solution
            var clientIp = GetClientIpAddress(context);
            var now = DateTime.UtcNow;
            var key = $"{clientIp}_{now:yyyy-MM-dd-HH-mm}";

            lock (_lock)
            {
                // Clean old entries
                var keysToRemove = _requestCounts.Keys
                    .Where(k => _requestCounts[k] < now.AddMinutes(-1))
                    .ToList();

                foreach (var keyToRemove in keysToRemove)
                {
                    _requestCounts.Remove(keyToRemove);
                }

                // Check current requests
                if (_requestCounts.ContainsKey(key))
                {
                    if (_requestCounts[key] > now.AddSeconds(-60)) // 1 minute window
                    {
                        var requestCount = _requestCounts.Count(kv => kv.Key.StartsWith(clientIp));
                        if (requestCount > 100) // 100 requests per minute
                        {
                            _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
                            return false;
                        }
                    }
                }

                _requestCounts[key] = now;
                return true;
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP headers
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
            {
                var ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(ip))
                    return ip;
            }

            if (context.Request.Headers.TryGetValue("X-Real-IP", out StringValues realIp))
            {
                return realIp.FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private void LogSecurityInfo(HttpContext context)
        {
            var request = context.Request;
            var clientIp = GetClientIpAddress(context);

            _logger.LogDebug("Security Info - IP: {ClientIP}, Method: {Method}, Path: {Path}, UserAgent: {UserAgent}",
                clientIp,
                request.Method,
                request.Path,
                request.Headers["User-Agent"].FirstOrDefault() ?? "unknown");

            // Log suspicious patterns
            var suspiciousPatterns = new[] { "script", "javascript", "eval", "onload", "onerror" };
            var queryString = request.QueryString.ToString().ToLowerInvariant();

            if (suspiciousPatterns.Any(pattern => queryString.Contains(pattern)))
            {
                _logger.LogWarning("Suspicious query string detected from IP: {ClientIP}, Query: {Query}",
                    clientIp, request.QueryString);
            }
        }
    }
}