using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.Data;
using ProgressTrackingSystem.Data.Entities;
using ProgressTrackingSystem.DTOs.Webhooks;
using ProgressTrackingSystem.Services.Interfaces;

namespace ProgressTrackingSystem.Services
{
    /// <summary>
    /// Service for managing course enrollments, syncing with external enrollment/payment API.
    /// </summary>
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ProgressDbContext _dbContext;
        private readonly IUmsApiService _umsApiService;
        private readonly ICmsApiService _cmsApiService;
        private readonly ICacheService _cacheService;
        private readonly IAuditLogService _auditLogService;
        private readonly HttpClient _enrollmentApiClient;
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(ProgressDbContext dbContext, IUmsApiService umsApiService,
            ICmsApiService cmsApiService, ICacheService cacheService, IAuditLogService auditLogService,
            IHttpClientFactory httpClientFactory, ILogger<EnrollmentService> logger)
        {
            _dbContext = dbContext;
            _umsApiService = umsApiService;
            _cmsApiService = cmsApiService;
            _cacheService = cacheService;
            _auditLogService = auditLogService;
            _enrollmentApiClient = httpClientFactory.CreateClient("EnrollmentApiClient");
            _logger = logger;
        }

        /// <summary>
        /// Syncs enrollments for a user with the external enrollment/payment API.
        /// </summary>
        public async Task SyncEnrollmentsAsync(string userId)
        {
            try
            {
                var user = await _umsApiService.GetUserProfileAsync(userId);
                if (user == null || user.Role != "Student")
                    throw new UnauthorizedAccessException("Only Students can have enrollments.");

                var response = await _enrollmentApiClient.GetAsync($"payments/pay/enrollments/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch enrollments for user {UserId}: {StatusCode}", userId, response.StatusCode);
                    throw new HttpRequestException($"Enrollment API error: {response.StatusCode}");
                }
                var externalCourseIds = await response.Content.ReadFromJsonAsync<List<string>>();
                if (externalCourseIds == null)
                {
                    _logger.LogWarning("No enrollments found for user {UserId}", userId);
                    externalCourseIds = new List<string>();
                }

                var validCourseIds = new List<string>();
                foreach (var courseId in externalCourseIds)
                {
                    try
                    {
                        var course = await _cmsApiService.GetCourseAsync(courseId);
                        if (course != null)
                            validCourseIds.Add(courseId);
                    }
                    catch (HttpRequestException)
                    {
                        _logger.LogWarning("Invalid course {CourseId} for user {UserId}", courseId, userId);
                    }
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingEnrollments = await _dbContext.Enrollments
                        .Where(e => e.UserId == userId)
                        .ToListAsync();

                    foreach (var courseId in validCourseIds.Except(existingEnrollments.Select(e => e.CourseId)))
                    {
                        _dbContext.Enrollments.Add(new Enrollment
                        {
                            UserId = userId,
                            CourseId = courseId,
                            EnrollmentDate = DateTime.UtcNow,
                            Status = EnrollmentStatus.Active
                        });
                    }

                    foreach (var enrollment in existingEnrollments.Where(e => !validCourseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.Active))
                    {
                        enrollment.Status = EnrollmentStatus.Dropped;
                    }

                    await _dbContext.SaveChangesAsync();
                    await _cacheService.RemoveAsync($"TotalEnrolledCourses_{userId}");
                    await _auditLogService.LogAsync(userId, "EnrollmentsSynced", $"Courses: {string.Join(",", validCourseIds)}");
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync enrollments for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets the total number of active enrolled courses for a user.
        /// </summary>
        public async Task<int> GetTotalEnrolledCoursesAsync(string userId)
        {
            var cacheKey = $"TotalEnrolledCourses_{userId}";
            var cachedCount = await _cacheService.GetAsync<int?>(cacheKey);
            if (cachedCount.HasValue) return cachedCount.Value;

            await SyncEnrollmentsAsync(userId);
            var count = await _dbContext.Enrollments
                .CountAsync(e => e.UserId == userId && e.Status == EnrollmentStatus.Active);

            await _cacheService.SetAsync(cacheKey, count, TimeSpan.FromMinutes(5));
            return count;
        }

        /// <summary>
        /// Processes enrollment webhook updates from the enrollment/payment API.
        /// </summary>
        public async Task HandleEnrollmentWebhookAsync(EnrollmentWebhookDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.UserId) || string.IsNullOrEmpty(dto.CourseId) || string.IsNullOrEmpty(dto.Action))
                    throw new ArgumentException("Invalid webhook data");

                var user = await _umsApiService.GetUserProfileAsync(dto.UserId);
                if (user == null || user.Role != "Student")
                    throw new UnauthorizedAccessException("Only Students can have enrollments.");

                var course = await _cmsApiService.GetCourseAsync(dto.CourseId);
                if (course == null)
                    throw new ArgumentException($"Course {dto.CourseId} not found.");

                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var enrollment = await _dbContext.Enrollments
                        .FirstOrDefaultAsync(e => e.UserId == dto.UserId && e.CourseId == dto.CourseId);

                    switch (dto.Action.ToLower())
                    {
                        case "enroll":
                            if (enrollment == null)
                            {
                                _dbContext.Enrollments.Add(new Enrollment
                                {
                                    UserId = dto.UserId,
                                    CourseId = dto.CourseId,
                                    EnrollmentDate = DateTime.UtcNow,
                                    Status = EnrollmentStatus.Active
                                });
                            }
                            else if (enrollment.Status != EnrollmentStatus.Active)
                            {
                                enrollment.Status = EnrollmentStatus.Active;
                                enrollment.EnrollmentDate = DateTime.UtcNow;
                            }
                            break;
                        case "drop":
                            if (enrollment != null && enrollment.Status == EnrollmentStatus.Active)
                            {
                                enrollment.Status = EnrollmentStatus.Dropped;
                            }
                            break;
                        default:
                            throw new ArgumentException($"Unsupported action: {dto.Action}");
                    }

                    await _dbContext.SaveChangesAsync();
                    await _cacheService.RemoveAsync($"TotalEnrolledCourses_{dto.UserId}");
                    await _auditLogService.LogAsync(dto.UserId, "EnrollmentWebhookProcessed", $"CourseId: {dto.CourseId}, Action: {dto.Action}");
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process enrollment webhook for user {UserId}, course {CourseId}", dto.UserId, dto.CourseId);
                throw;
            }
        }
    }
}