using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProgressTrackingSystem.Data;
using ProgressTrackingSystem.Data.Entities;
using ProgressTrackingSystem.Services.Interfaces;

namespace ProgressTrackingSystem.Services
{
    /// <summary>
    /// Service for logging audit events.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly ProgressDbContext _dbContext;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(ProgressDbContext dbContext, ILogger<AuditLogService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Logs an audit event to the database.
        /// </summary>
        public async Task LogAsync(string userId, string action, string details)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };
                _dbContext.AuditLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit event for user {UserId}, action {Action}", userId, action);
            }
        }
    }
}