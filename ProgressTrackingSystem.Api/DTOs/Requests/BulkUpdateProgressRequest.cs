using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ProgressTrackingSystem.DTOs.Requests
{
    /// <summary>
    /// Request model for bulk updating video progress.
    /// </summary>
    [SwaggerSchema(
        Title = "Bulk Update Progress Request",
        Description = "Request containing multiple video progress updates"
    )]
    public record BulkUpdateProgressRequest
    {
        /// <summary>
        /// List of video progress updates to process in a single request
        /// </summary>
        /// <example>[{ "videoId": "vid123", "currentTimeSeconds": 120, "totalDurationSeconds": 300, "markAsCompleted": false }]</example>
        [Required]
        [MinLength(1)]
        [SwaggerSchema(Description = "List of video progress updates to process")]
        public required List<UpdateVideoProgressRequest> VideoProgresses { get; init; }
    }
}