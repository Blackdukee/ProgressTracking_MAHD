using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.Services.Interfaces;
using ProgressTrackingSystem.DTOs.Webhooks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace ProgressTrackingSystem.Controllers
{
    /// <summary>
    /// Controller for handling external system webhooks
    /// </summary>
    [Route("api/v1/progress/[controller]")]
    [ApiController]
    [SwaggerTag("Webhook endpoints for external system integrations")]
    public class WebhookController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IEnrollmentService enrollmentService,
            IConfiguration configuration,
            ILogger<WebhookController> logger)
        {
            _enrollmentService = enrollmentService;
            _configuration = configuration;
            _logger = logger;
        }        /// <summary>
        /// Processes enrollment updates from the Enrollment API
        /// </summary>
        /// <param name="dto">Enrollment webhook data containing user and course information</param>
        /// <returns>200 OK if processed successfully, 401 Unauthorized if invalid server key, 500 for internal errors</returns>
        /// <remarks>
        /// This endpoint handles enrollment events from the enrollment system when users are enrolled in or unenrolled from courses.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/progress/webhook/enrollment-updated
        ///     {
        ///       "userId": "user123",
        ///       "courseId": "course456",
        ///       "enrollmentStatus": "enrolled",
        ///       "enrollmentDate": "2025-06-14T14:30:00Z"
        ///     }
        /// </remarks>
        [HttpPost("enrollment-updated")]
        [SwaggerOperation(
            Summary = "Process enrollment updates",
            Description = "Handles enrollment webhooks from the Enrollment API to track when users are enrolled in or unenrolled from courses",
            OperationId = "HandleEnrollmentWebhook",
            Tags = new[] { "Webhooks" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Webhook processed successfully")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid server key authentication")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error processing the webhook")]
        public async Task<IActionResult> HandleEnrollmentWebhook([FromBody][Required] EnrollmentWebhookDto dto)
        {
            var expectedServerKey = _configuration["EnrollmentApi:ServerKey"];
            var receivedServerKey = Request.Headers["X-Server-Key"].ToString();

            if (!receivedServerKey.Equals(expectedServerKey, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid server key for enrollment webhook");
                return Unauthorized();
            }

            try
            {
                await _enrollmentService.HandleEnrollmentWebhookAsync(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process enrollment webhook for user {UserId}, course {CourseId}", dto.UserId, dto.CourseId);
                return StatusCode(500);
            }
        }
    }
}