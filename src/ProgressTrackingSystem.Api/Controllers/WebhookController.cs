using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.Services.Interfaces;
using ProgressTrackingSystem.DTOs.Webhooks;

namespace ProgressTrackingSystem.Controllers
{
    [Route("api/v1/progress/[controller]")]
    [ApiController]
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
        }

        [HttpPost("enrollment-updated")]
        public async Task<IActionResult> HandleEnrollmentWebhook([FromBody] EnrollmentWebhookDto dto)
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