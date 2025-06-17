using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ProgressTrackingSystem.DTOs.Requests
{
    /// <summary>
    /// Request model for updating video progress.
    /// </summary>
    [SwaggerSchema(
        Title = "Update Video Progress Request",
        Description = "Details about a user's progress in watching a video"
    )]
    public record UpdateVideoProgressRequest
    {
        /// <summary>
        /// Current playback position in seconds
        /// </summary>
        /// <example>120</example>
        [Required]
        [Range(0, int.MaxValue)]
        [SwaggerSchema(Description = "Current playback position in seconds")]
        public int CurrentTimeSeconds { get; init; }

        /// <summary>
        /// Total duration of the video in seconds
        /// </summary>
        /// <example>300</example>
        [Required]
        [Range(1, int.MaxValue)]
        [SwaggerSchema(Description = "Total duration of the video in seconds")]
        public int TotalDurationSeconds { get; init; }

        /// <summary>
        /// Whether to mark the video as fully completed regardless of progress percentage
        /// </summary>
        /// <example>false</example>
        [SwaggerSchema(Description = "Flag to mark video as fully completed regardless of progress")]
        public bool MarkAsCompleted { get; init; }
    }
}