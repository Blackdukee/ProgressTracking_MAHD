using ProgressTrackingSystem.DTOs.Requests;
using ProgressTrackingSystem.DTOs.Responses;

namespace ProgressTrackingSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for managing user progress data.
    /// </summary>
    public interface IProgressService
    {
        Task<VideoProgressDto> UpdateVideoProgressAsync(string userId, string videoId, UpdateVideoProgressRequest request);
        Task<ProgressSummaryDto> GetProgressSummaryAsync(string userId);
    }
}