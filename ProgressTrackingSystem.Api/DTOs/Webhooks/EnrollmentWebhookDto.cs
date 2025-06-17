using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ProgressTrackingSystem.DTOs.Webhooks
{
    /// <summary>
    /// Webhook payload for enrollment updates from the payments / enrollments API.
    /// </summary>
    [SwaggerSchema(
        Title = "Enrollment Webhook",
        Description = "Data received when a user is enrolled in or dropped from a course"
    )]
    public record EnrollmentWebhookDto
    {
        /// <summary>
        /// Unique identifier of the user who is enrolled/unenrolled
        /// </summary>
        /// <example>user123</example>
        [Required]
        [SwaggerSchema(Description = "Unique identifier of the user")]
        public required string UserId { get; init; }
        
        /// <summary>
        /// Unique identifier of the course being enrolled/unenrolled
        /// </summary>
        /// <example>course456</example>
        [Required]
        [SwaggerSchema(Description = "Unique identifier of the course")]
        public required string CourseId { get; init; }
          /// <summary>
        /// Type of enrollment action - either "enroll" or "drop"
        /// </summary>
        /// <example>enroll</example>
        [Required]
        [SwaggerSchema(
            Description = "Type of enrollment action",
            Format = "string"
        )]
        public required string Action { get; init; }
    }
}