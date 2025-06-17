namespace ProgressTrackingSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for interacting with the User Management System API.
    /// </summary>
    public interface IUmsApiService
    {
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
    }

    public record UserProfileDto(string Id, string Email, string Role);
}
