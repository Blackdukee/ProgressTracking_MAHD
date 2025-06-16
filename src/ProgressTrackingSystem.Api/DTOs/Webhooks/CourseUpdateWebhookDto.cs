using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ProgressTrackingSystem.DTOs.Webhooks
{
    /// <summary>
    /// Webhook payload for course updates from CMS.
    /// </summary>
    [SwaggerSchema(
        Title = "Course Update Webhook",
        Description = "Data received when a course is updated in the CMS"
    )]
    public record CourseUpdateWebhookDto
    {
        /// <summary>
        /// Unique identifier of the course that was updated
        /// </summary>
        /// <example>course123</example>
        [Required]
        [SwaggerSchema(Description = "Unique identifier of the course")]
        public required string CourseId { get; init; }
    }
}