namespace ProgressTrackingSystem.DTOs.Requests
{
    /// <summary>
    /// Request model for bulk updating video progress.
    /// </summary>
    public record BulkUpdateProgressRequest
    {
        public required List<UpdateVideoProgressRequest> VideoProgresses { get; init; }
    }
}