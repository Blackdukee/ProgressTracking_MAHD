namespace ProgressTrackingSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for caching operations.
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiry);
        Task RemoveAsync(string key);
    }
}
