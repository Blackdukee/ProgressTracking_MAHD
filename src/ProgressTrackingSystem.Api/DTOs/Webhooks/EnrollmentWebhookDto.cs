namespace ProgressTrackingSystem.DTOs.Webhooks
{
    /// <summary>
    /// Webhook payload for enrollment updates from the  payments / pay / enrollments API.
    /// </summary>
    public record EnrollmentWebhookDto
    {
        public required string UserId { get; init; }
        public required string CourseId { get; init; }
        public required string Action { get; init; } // "enroll" or "drop"
    }
}