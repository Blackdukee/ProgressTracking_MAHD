using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using ProgressTrackingSystem.Middleware;

namespace ProgressTrackingSystem.Extensions
{
    /// <summary>
    /// Extension methods for configuring service collection and middleware.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static void AddCustomMiddleware(this IServiceCollection services)
        {
            // Add additional middleware configurations if needed
        }

        public static void UseCustomMiddleware(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<TokenValidationMiddleware>();
        }
    }
}