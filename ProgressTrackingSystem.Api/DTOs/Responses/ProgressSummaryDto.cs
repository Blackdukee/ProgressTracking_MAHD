using ProgressTrackingSystem.DTOs.Responses;

namespace ProgressTrackingSystem.DTOs.Responses
{
    /// <summary>
    /// Data transfer object for user progress summary.
    /// </summary>
    public record ProgressSummaryDto
    {
        public required string UserId { get; init; }
        public int TotalCoursesEnrolled { get; init; }
        public int CompletedCourses { get; init; }
        public int TotalVideosWatched { get; init; }
        public double TotalWatchTimeHours { get; init; }
       
        public required List<CourseProgressDto> RecentCourses { get; init; }
    }
}