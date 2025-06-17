using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.Services.Interfaces;

namespace ProgressTrackingSystem.Services
{
    /// <summary>
    /// Service for interacting with the User Management System API.
    /// </summary>
    public class UmsApiService : IUmsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UmsApiService> _logger;

        public UmsApiService(IHttpClientFactory httpClientFactory, ILogger<UmsApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("UmsApiClient");
            _logger = logger;
        }

        /// <summary>
        /// Retrieves user profile from UMS.
        /// </summary>
        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/ums/profile?userId={userId}");
                response.EnsureSuccessStatusCode();
                var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                if (profile == null)
                {
                    _logger.LogWarning("No profile found for userId {UserId}", userId);
                    return null;
                }
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user profile for user {UserId}", userId);
                throw;
            }
        }
    }
}