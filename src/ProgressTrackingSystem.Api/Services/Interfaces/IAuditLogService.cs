namespace ProgressTrackingSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for logging audit events.
    /// </summary>
    public interface IAuditLogService
    {
        Task LogAsync(string userId, string action, string details);
    }
}