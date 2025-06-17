namespace ProgressTrackingSystem.Data.Entities
{
    /// <summary>
    /// Represents an audit log entry for tracking system actions.
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Action { get; set; }
        public required string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
