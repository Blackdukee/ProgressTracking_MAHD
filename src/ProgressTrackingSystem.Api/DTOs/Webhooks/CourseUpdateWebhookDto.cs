namespace ProgressTrackingSystem.DTOs.Webhooks
{
    /// <summary>
    /// Webhook payload for course updates from CMS.
    /// </summary>
    public record CourseUpdateWebhookDto
    {
        public required string CourseId { get; init; }
    }
}