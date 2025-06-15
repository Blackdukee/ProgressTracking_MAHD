namespace ProgressTrackingSystem.DTOs.Responses
{
    /// <summary>
    /// Data transfer object for course progress details.
    /// </summary>
    public record CourseProgressDto
    {
        public int Id { get; init; }
        public required string CourseId { get; init; }
        public required string CourseTitle { get; init; }
        public int CompletedVideos { get; init; }
        public int TotalVideos { get; init; }
        public double CompletionPercentage { get; init; }
        public double TotalWatchTimeSeconds { get; init; }
        public DateTime LastAccessed { get; init; }
        public List<VideoProgressDto> VideoProgresses { get; init; } = new();
    }
}