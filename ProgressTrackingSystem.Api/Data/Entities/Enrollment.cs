namespace ProgressTrackingSystem.Data.Entities
{
    /// <summary>
    /// Represents a user's enrollment in a course.
    /// </summary>
    public class Enrollment
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    }

    public enum EnrollmentStatus
    {
        Active,
        Dropped,
        Completed
    }
}