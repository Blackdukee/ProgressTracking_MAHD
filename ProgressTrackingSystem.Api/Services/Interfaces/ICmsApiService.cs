namespace ProgressTrackingSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for interacting with the Content Management System API.
    /// </summary>
    public interface ICmsApiService
    {
        Task<CourseDto?> GetCourseAsync(string courseId);
        Task<VideoDto?> GetVideoAsync(string videoId);
        Task<List<SectionDto>?> GetSectionsAsync(string courseId);
        Task<List<VideoDto>?> GetVideosAsync(string sectionId);
    }

    public record CourseDto(string Id, string Title);
    public record VideoDto(string Id, string Title, string CourseId, string SectionId, int DurationSeconds);
    public record SectionDto(string Id, string Title, string CourseId);
}