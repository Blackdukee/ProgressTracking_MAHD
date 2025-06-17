namespace ProgressTrackingSystem.DTOs.Responses
{
    /// <summary>
    /// Data transfer object for video progress details.
    /// </summary>
    public record VideoProgressDto
    {
        public int Id { get; init; }
        public required string VideoId { get; init; }
        public required string VideoTitle { get; init; }
        public int CurrentTimeSeconds { get; init; }
        public double CompletionPercentage { get; init; }
        public bool IsCompleted { get; init; }
        public DateTime LastWatched { get; init; }
    }
}

