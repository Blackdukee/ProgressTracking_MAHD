namespace ProgressTrackingSystem.Data.Entities
{
    /// <summary>
    /// Represents a user's progress for a specific video.
    /// </summary>
    public class VideoProgress
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string VideoId { get; set; }
        public int CurrentTimeSeconds { get; set; }
        public double CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime LastWatched { get; set; }
    }
}
