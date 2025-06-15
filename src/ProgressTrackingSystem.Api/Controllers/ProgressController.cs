using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.DTOs.Requests;
using ProgressTrackingSystem.DTOs.Responses;
using ProgressTrackingSystem.Services.Interfaces;

namespace ProgressTrackingSystem.Controllers
{
    /// <summary>
    /// Controller for managing user progress data.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;
        private readonly ILogger<ProgressController> _logger;

        public ProgressController(IProgressService progressService, ILogger<ProgressController> logger)
        {
            _progressService = progressService;
            _logger = logger;
        }

        /// <summary>
        /// Updates video progress for a user and triggers course progress update.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="videoId">The ID of the video.</param>
        /// <param name="request">Video progress update details.</param>
        /// <returns>Updated video progress details.</returns>
        [HttpPost("video/{userId}/{videoId}")]
        public async Task<ActionResult<VideoProgressDto>> UpdateVideoProgress(string userId, string videoId, [FromBody] UpdateVideoProgressRequest request)
        {
            try
            {
                var progress = await _progressService.UpdateVideoProgressAsync(userId, videoId, request);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update video progress for user {UserId}, video {VideoId}", userId, videoId);
                return StatusCode(500, new { Success = false, Message = "Failed to update video progress" });
            }
        }

        /// <summary>
        /// Retrieves the progress summary for a user, including total enrolled courses.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Progress summary with metrics like "Courses Enrolled: [X]".</returns>
        [HttpGet("summary/{userId}")]
        public async Task<ActionResult<ProgressSummaryDto>> GetProgressSummary(string userId)
        {
            try
            {
                var summary = await _progressService.GetProgressSummaryAsync(userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch progress summary for user {UserId}", userId);
                return StatusCode(500, new { Success = false, Message = "Failed to fetch progress summary" });
            }
        }
    }
}