using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ProgressTrackingSystem.DTOs.Webhooks
{
    /// <summary>
    /// Webhook payload for video updates from CMS.
    /// </summary>
    [SwaggerSchema(
        Title = "Video Update Webhook",
        Description = "Data received when a video is updated in the CMS"
    )]
    public record VideoUpdateWebhookDto
    {
        /// <summary>
        /// Unique identifier of the video that was updated
        /// </summary>
        /// <example>video789</example>
        [Required]
        [SwaggerSchema(Description = "Unique identifier of the video")]
        public required string VideoId { get; init; }
    }
}