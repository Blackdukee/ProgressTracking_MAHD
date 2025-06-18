using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProgressTrackingSystem.Middleware
{
    /// <summary>
    /// Middleware for validating JWT tokens with an external User Management Service.
    /// </summary>
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        private class UmsUser
        {
            public long Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
        }

        private class UmsValidationResponse
        {
            public bool Valid { get; set; }
            public UmsUser User { get; set; } = new UmsUser();
        }

        public TokenValidationMiddleware(
            RequestDelegate next, 
            ILogger<TokenValidationMiddleware> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {            // Skip authentication for webhook endpoints and Swagger-related paths
            if (context.Request.Path.StartsWithSegments("/api/v1/progress/webhook") || 
                context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/swagger-ui"))
            {
                _logger.LogInformation("Skipping authentication for path: {Path}", context.Request.Path);
                await _next(context);
                return;
            }

            try
            {
                string authHeader = context.Request.Headers.Authorization.ToString();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    await WriteErrorResponseAsync(context, "Authorization header missing or invalid", StatusCodes.Status401Unauthorized);
                    return;
                }

                string token = authHeader.Split(' ')[1];
                
                // Validate token with UMS service
                var userServiceUrl = _configuration["Ums:BaseUrl"] ?? "http://3.70.227.2:5003/api/v1/ums";
                var serverKey = _configuration["Ums:ServerKey"];

                if (string.IsNullOrEmpty(serverKey))
                {
                    _logger.LogError("UMS server key not configured");
                    await WriteErrorResponseAsync(context, "Authentication service configuration error", StatusCodes.Status500InternalServerError);
                    return;
                }

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("X-Service-Key", serverKey);

                var tokenValidationRequest = new { token };
                var response = await httpClient.PostAsJsonAsync(
                    $"{userServiceUrl}/auth/validate", 
                    tokenValidationRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to validate token with UMS. Status code: {StatusCode}", response.StatusCode);
                    await WriteErrorResponseAsync(context, "Failed to validate token", StatusCodes.Status401Unauthorized);
                    return;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var validationResult = JsonSerializer.Deserialize<UmsValidationResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (validationResult == null || !validationResult.Valid)
                {
                    _logger.LogWarning("Invalid or expired token received");
                    await WriteErrorResponseAsync(context, "Invalid or expired token", StatusCodes.Status401Unauthorized);
                    return;
                }

                // Add user info to the HttpContext items
                context.Items["UserInfo"] = new UserInfo
                {
                    Id = validationResult.User.Id.ToString(),
                    Email = validationResult.User.Email,
                    FirstName = validationResult.User.FirstName,
                    LastName = validationResult.User.LastName,
                    Role = validationResult.User.Role
                };
                _logger.LogInformation("Token validated successfully for user: {UserId}", validationResult.User.Id);
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in token validation middleware");
                await WriteErrorResponseAsync(context, "Authentication error", StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task WriteErrorResponseAsync(HttpContext context, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                Success = false,
                Message = message
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    /// <summary>
    /// Response model from token validation endpoint
    /// </summary>
    internal class TokenValidationResponse
    {
        public bool Valid { get; set; }
        public UserInfo User { get; set; } = new UserInfo();
    }

    /// <summary>
    /// User information from token validation
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
