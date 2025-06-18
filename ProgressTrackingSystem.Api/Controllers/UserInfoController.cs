using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.Extensions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace ProgressTrackingSystem.Controllers
{
    /// <summary>
    /// Controller to demonstrate getting current user info.
    /// </summary>
    [Route("api/v1/progress/user")]
    [ApiController]
    [Produces("application/json")]
    public class UserInfoController : ControllerBase
    {
        private readonly ILogger<UserInfoController> _logger;

        public UserInfoController(ILogger<UserInfoController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the current authenticated user information.
        /// </summary>
        /// <returns>User information from the token.</returns>
        /// <remarks>
        /// This endpoint demonstrates how to access the user information
        /// that was extracted from the token by the TokenValidationMiddleware.
        /// </remarks>
        [HttpGet("info")]
        [SwaggerOperation(
            Summary = "Get current user info",
            Description = "Returns the authenticated user's information from the validated token",
            OperationId = "GetCurrentUser",
            Tags = new[] { "User" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved user information")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public IActionResult GetCurrentUser()
        {
            _logger.LogInformation("Retrieving current user information");
            var currentUser = HttpContext.GetCurrentUser();
            
            if (currentUser == null)
            {
                return Unauthorized(new { Success = false, Message = "User not authenticated" });
            }
            
            _logger.LogInformation("Current user retrieved: {UserId}", currentUser.Id);
            return Ok(new
            {
                Success = true,
                User = new
                {
                    Id = currentUser.Id,
                    Email = currentUser.Email,
                    FristName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    Role = currentUser.Role
                }
            });
        }
    }
}
