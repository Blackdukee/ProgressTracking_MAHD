namespace ProgressTrackingSystem.DTOs.Requests
{
    /// <summary>
    /// Request model for updating video progress.
    /// </summary>
    public record UpdateVideoProgressRequest
    {
        public int CurrentTimeSeconds { get; init; }
        public int TotalDurationSeconds { get; init; }
        public bool MarkAsCompleted { get; init; }
    }
}