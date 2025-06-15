namespace ProgressTrackingSystem.Data.Entities
{
    /// <summary>
    /// Represents a user's progress for a specific course.
    /// </summary>
    public class CourseProgress
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string CourseId { get; set; }
        public int CompletedVideos { get; set; }
        public int TotalVideos { get; set; }
        public double CompletionPercentage { get; set; }
        public double TotalWatchTimeSeconds { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}