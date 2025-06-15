namespace ProgressTrackingSystem.DTOs.Webhooks
{
    /// <summary>
    /// Webhook payload for video updates from CMS.
    /// </summary>
    public record VideoUpdateWebhookDto
    {
        public required string VideoId { get; init; }
    }
}