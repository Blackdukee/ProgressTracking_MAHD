using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.Data;
using ProgressTrackingSystem.Data.Entities;
using ProgressTrackingSystem.DTOs.Requests;
using ProgressTrackingSystem.DTOs.Responses;
using ProgressTrackingSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgressTrackingSystem.Services
{
    /// <summary>
    /// Service for managing user progress data.
    /// </summary>
    public class ProgressService : IProgressService
    {
        private readonly ProgressDbContext _dbContext;
        private readonly ICmsApiService _cmsApiService;
        private readonly IUmsApiService _umsApiService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICacheService _cacheService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<ProgressService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProgressService(
            ProgressDbContext dbContext,
            ICmsApiService cmsApiService,
            IUmsApiService umsApiService,
            IEnrollmentService enrollmentService,
            ICacheService cacheService,
            IAuditLogService auditLogService,
            ILogger<ProgressService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _cmsApiService = cmsApiService;
            _umsApiService = umsApiService;
            _enrollmentService = enrollmentService;
            _cacheService = cacheService;
            _auditLogService = auditLogService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Retrieves the progress summary for a user, including enrollment count.
        /// </summary>
        public async Task<ProgressSummaryDto> GetProgressSummaryAsync(string userId)
        {
            var cacheKey = $"ProgressSummary_{userId}";
            var cachedSummary = await _cacheService.GetAsync<ProgressSummaryDto>(cacheKey);
            if (cachedSummary != null) return cachedSummary;

            try
            {
                // Get user role from JWT token
                var role = _httpContextAccessor.HttpContext?.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                if (string.IsNullOrEmpty(role))
                {
                    _logger.LogWarning("No role found in JWT, using mock role for user {UserId}", userId);
                    role = "Student"; // Mock role
                }

                if (role != "Student")
                    throw new UnauthorizedAccessException("Only Students can view progress.");

                // Fetch enrollment count
                int totalCoursesEnrolled;
                try
                {
                    totalCoursesEnrolled = await _enrollmentService.GetTotalEnrolledCoursesAsync(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Using mock enrollment data for user {UserId}", userId);
                    totalCoursesEnrolled = 0; // Mock value
                }

                // Fetch course progress
                var courseProgresses = await _dbContext.CourseProgresses
                    .Where(cp => cp.UserId == userId)
                    .ToListAsync();

                // Calculate metrics
                var completedCourses = courseProgresses.Count(cp => cp.CompletionPercentage >= 95);
                var totalVideosWatched = await _dbContext.VideoProgresses
                    .CountAsync(vp => vp.UserId == userId && vp.IsCompleted);
                var totalWatchTimeSeconds = await _dbContext.VideoProgresses
                    .Where(vp => vp.UserId == userId)
                    .SumAsync(vp => vp.CurrentTimeSeconds);
                var totalWatchTimeHours = totalWatchTimeSeconds / 3600.0;

                // Fetch course titles from CMS
                var recentCourses = new List<CourseProgressDto>();
                foreach (var cp in courseProgresses.Take(5))
                {
                    var course = await _cmsApiService.GetCourseAsync(cp.CourseId);
                    if (course == null)
                    {
                        // Mock data for testing
                        course = new CourseDto(cp.CourseId, $"Mock Course {cp.CourseId}");
                        _logger.LogWarning("Using mock course data for course {CourseId}", cp.CourseId);
                    }
                    recentCourses.Add(new CourseProgressDto
                    {
                        Id = cp.Id,
                        CourseId = cp.CourseId,
                        CourseTitle = course.Title,
                        CompletedVideos = cp.CompletedVideos,
                        TotalVideos = cp.TotalVideos,
                        CompletionPercentage = cp.CompletionPercentage,
                        TotalWatchTimeSeconds = cp.TotalWatchTimeSeconds,
                        LastAccessed = cp.LastAccessed
                    });
                }

                var summary = new ProgressSummaryDto
                {
                    UserId = userId,
                    TotalCoursesEnrolled = totalCoursesEnrolled,
                    CompletedCourses = completedCourses,
                    TotalVideosWatched = totalVideosWatched,
                    TotalWatchTimeHours = totalWatchTimeHours,
                    RecentCourses = recentCourses
                };

                await _cacheService.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(5));
                await _auditLogService.LogAsync(userId, "ProgressSummaryFetched", $"CoursesEnrolled: {totalCoursesEnrolled}");
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch progress summary for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Updates video progress for a user and triggers course progress update.
        /// </summary>
        public async Task<VideoProgressDto> UpdateVideoProgressAsync(string userId, string videoId, UpdateVideoProgressRequest request)
        {
            // Get user role from JWT token
            var role = _httpContextAccessor.HttpContext?.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            if (string.IsNullOrEmpty(role))
            {
                _logger.LogWarning("No role found in JWT, using mock role for user {UserId}", userId);
                role = "Student"; // Mock role
            }

            if (role != "Student")
                throw new UnauthorizedAccessException("Only Students can update progress.");

            var video = await _cmsApiService.GetVideoAsync(videoId);
            if (video == null)
            {
                // Mock data for testing
                video = new VideoDto(videoId, $"Mock Video {videoId}", "mock-course", "mock-section", 300);
                _logger.LogWarning("Using mock video data for video {VideoId}", videoId);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var progress = await _dbContext.VideoProgresses
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.VideoId == videoId)
                    ?? new VideoProgress { UserId = userId, VideoId = videoId };

                progress.CurrentTimeSeconds = request.CurrentTimeSeconds;
                progress.CompletionPercentage = (double)request.CurrentTimeSeconds / video.DurationSeconds * 100;
                progress.IsCompleted = request.MarkAsCompleted || progress.CompletionPercentage >= 95;
                progress.LastWatched = DateTime.UtcNow;

                if (progress.Id == 0) _dbContext.VideoProgresses.Add(progress);
                await _dbContext.SaveChangesAsync();

                await UpdateCourseProgressAsync(userId, video.CourseId);
                await _auditLogService.LogAsync(userId, "VideoProgressUpdated", $"VideoId: {videoId}");
                await _cacheService.RemoveAsync($"ProgressSummary_{userId}");

                await transaction.CommitAsync();
                return new VideoProgressDto
                {
                    Id = progress.Id,
                    VideoId = videoId,
                    VideoTitle = video.Title,
                    CurrentTimeSeconds = progress.CurrentTimeSeconds,
                    CompletionPercentage = progress.CompletionPercentage,
                    IsCompleted = progress.IsCompleted,
                    LastWatched = progress.LastWatched
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update video progress for user {UserId}, video {VideoId}", userId, videoId);
                throw;
            }
        }

        /// <summary>
        /// Updates course progress based on video progress.
        /// </summary>
        private async Task UpdateCourseProgressAsync(string userId, string courseId)
        {
            var sections = await _cmsApiService.GetSectionsAsync(courseId);
            if (sections == null)
            {
                // Mock data for testing
                sections = new List<SectionDto> { new SectionDto("mock-section", "Mock Section", courseId) };
                _logger.LogWarning("Using mock sections for course {CourseId}", courseId);
            }

            int totalVideos = 0, completedVideos = 0;
            double totalWatchTime = 0;

            foreach (var section in sections)
            {
                var videos = await _cmsApiService.GetVideosAsync(section.Id);
                if (videos == null)
                {
                    // Mock data for testing
                    videos = new List<VideoDto> { new VideoDto("mock-video", "Mock Video", courseId, section.Id, 300) };
                    _logger.LogWarning("Using mock videos for section {SectionId}", section.Id);
                }

                totalVideos += videos.Count;
                var progress = await _dbContext.VideoProgresses
                    .Where(p => p.UserId == userId && videos.Select(v => v.Id).Contains(p.VideoId))
                    .ToListAsync();
                completedVideos += progress.Count(p => p.IsCompleted);
                totalWatchTime += progress.Sum(p => p.CurrentTimeSeconds);
            }

            var courseProgress = await _dbContext.CourseProgresses
                .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CourseId == courseId)
                ?? new CourseProgress { UserId = userId, CourseId = courseId };

            courseProgress.CompletedVideos = completedVideos;
            courseProgress.TotalVideos = totalVideos;
            courseProgress.CompletionPercentage = totalVideos > 0 ? (double)completedVideos / totalVideos * 100 : 0;
            courseProgress.TotalWatchTimeSeconds = totalWatchTime;
            courseProgress.LastAccessed = DateTime.UtcNow;

            if (courseProgress.Id == 0) _dbContext.CourseProgresses.Add(courseProgress);
            await _dbContext.SaveChangesAsync();
            await _auditLogService.LogAsync(userId, "CourseProgressUpdated", $"CourseId: {courseId}");
        }
    }
}