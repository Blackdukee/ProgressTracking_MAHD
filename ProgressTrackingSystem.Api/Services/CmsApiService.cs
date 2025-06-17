using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.Services.Interfaces;

namespace ProgressTrackingSystem.Services
{
    /// <summary>
    /// Service for interacting with the Content Management System API.
    /// </summary>
    public class CmsApiService : ICmsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CmsApiService> _logger;

        public CmsApiService(IHttpClientFactory httpClientFactory, ICacheService cacheService, ILogger<CmsApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("CmsApiClient");
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves course details from CMS.
        /// </summary>
        public async Task<CourseDto?> GetCourseAsync(string courseId)
        {
            var cacheKey = $"Course_{courseId}";
            var cachedCourse = await _cacheService.GetAsync<CourseDto>(cacheKey);
            if (cachedCourse != null) return cachedCourse;

            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/cms/get-all?courseId={courseId}");
                response.EnsureSuccessStatusCode();
                var course = await response.Content.ReadFromJsonAsync<CourseDto>();
                if (course == null)
                {
                    _logger.LogWarning("No course found for courseId {CourseId}", courseId);
                    return null;
                }
                await _cacheService.SetAsync(cacheKey, course, TimeSpan.FromHours(1));
                return course;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch course {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves video details from CMS.
        /// </summary>
        public async Task<VideoDto?> GetVideoAsync(string videoId)
        {
            var cacheKey = $"Video_{videoId}";
            var cachedVideo = await _cacheService.GetAsync<VideoDto>(cacheKey);
            if (cachedVideo != null) return cachedVideo;

            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/cms/video/get-video/{videoId}");
                response.EnsureSuccessStatusCode();
                var video = await response.Content.ReadFromJsonAsync<VideoDto>();
                if (video == null)
                {
                    _logger.LogWarning("No video found for videoId {VideoId}", videoId);
                    return null;
                }
                await _cacheService.SetAsync(cacheKey, video, TimeSpan.FromHours(1));
                return video;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch video {VideoId}", videoId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves sections for a course from CMS.
        /// </summary>
        public async Task<List<SectionDto>?> GetSectionsAsync(string courseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/cms/section/get-all/{courseId}");
                response.EnsureSuccessStatusCode();
                var sections = await response.Content.ReadFromJsonAsync<List<SectionDto>>();
                if (sections == null)
                {
                    _logger.LogWarning("No sections found for courseId {CourseId}", courseId);
                    return new List<SectionDto>();
                }
                return sections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch sections for course {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves videos for a section from CMS.
        /// </summary>
        public async Task<List<VideoDto>?> GetVideosAsync(string sectionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/cms/video/get-all/{sectionId}");
                response.EnsureSuccessStatusCode();
                var videos = await response.Content.ReadFromJsonAsync<List<VideoDto>>();
                if (videos == null)
                {
                    _logger.LogWarning("No videos found for sectionId {SectionId}", sectionId);
                    return new List<VideoDto>();
                }
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch videos for section {SectionId}", sectionId);
                throw;
            }
        }
    }
}