using ProgressTrackingSystem.DTOs.Webhooks;

namespace ProgressTrackingSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for managing course enrollments.
    /// </summary>
    public interface IEnrollmentService
    {
        Task SyncEnrollmentsAsync(string userId);
        Task<int> GetTotalEnrolledCoursesAsync(string userId);
        Task HandleEnrollmentWebhookAsync(EnrollmentWebhookDto dto);

    }
}