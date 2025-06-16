using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.DTOs.Requests;
using ProgressTrackingSystem.DTOs.Responses;
using ProgressTrackingSystem.Extensions;
using ProgressTrackingSystem.Services.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace ProgressTrackingSystem.Controllers
{
    /// <summary>
    /// Controller for managing user progress data.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("APIs for tracking and retrieving user learning progress")]
    [Produces("application/json")]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;
        private readonly ILogger<ProgressController> _logger;

        public ProgressController(IProgressService progressService, ILogger<ProgressController> logger)
        {
            _progressService = progressService;
            _logger = logger;
        }        /// <summary>
        /// Updates video progress for a user and triggers course progress update.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="videoId">The ID of the video.</param>
        /// <param name="request">Video progress update details.</param>
        /// <returns>Updated video progress details.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/progress/video/user123/vid456
        ///     {
        ///       "currentTimeSeconds": 120,
        ///       "totalDurationSeconds": 300,
        ///       "markAsCompleted": false
        ///     }
        ///
        /// </remarks>
        [HttpPost("video/{userId}/{videoId}")]
        [SwaggerOperation(
            Summary = "Update video progress",
            Description = "Updates the current watching progress of a video and automatically updates the overall course progress",
            OperationId = "UpdateVideoProgress",
            Tags = new[] { "Progress" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Video progress updated successfully", typeof(VideoProgressDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid progress data")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authorized")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<ActionResult<VideoProgressDto>> UpdateVideoProgress(
            [FromRoute][Required] string userId,
            [FromRoute][Required] string videoId,
            [FromBody][Required] UpdateVideoProgressRequest request)
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
        }        /// <summary>
        /// Retrieves the progress summary for a user, including total enrolled courses.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Progress summary with metrics like "Courses Enrolled: [X]".</returns>
        /// <remarks>
        /// Returns a comprehensive summary of the user's learning progress across all courses,
        /// including total watch time, completion rates, and recently accessed courses.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/v1/progress/summary/user123
        /// 
        /// </remarks>
        [HttpGet("summary/{userId}")]
        [SwaggerOperation(
            Summary = "Get user progress summary",
            Description = "Retrieves a summary of the user's progress across all enrolled courses",
            OperationId = "GetProgressSummary",
            Tags = new[] { "Progress" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Progress summary retrieved successfully", typeof(ProgressSummaryDto))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authorized")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<ActionResult<ProgressSummaryDto>> GetProgressSummary([FromRoute][Required] string userId)
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