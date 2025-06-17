using Microsoft.AspNetCore.Http;
using ProgressTrackingSystem.Middleware;

namespace ProgressTrackingSystem.Extensions
{
    /// <summary>
    /// Extension methods for HttpContext to easily access user information.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the current authenticated user information from the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <returns>User information or null if not authenticated</returns>
        public static UserInfo? GetCurrentUser(this HttpContext context)
        {
            if (context.Items.TryGetValue("UserInfo", out var userInfo))
            {
                return userInfo as UserInfo;
            }
            
            return null;
        }
    }
}
