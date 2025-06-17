using Microsoft.EntityFrameworkCore;
using ProgressTrackingSystem.Data.Entities;

namespace ProgressTrackingSystem.Data
{
    /// <summary>
    /// Entity Framework Core context for the Progress Tracking System database.
    /// </summary>
    public class ProgressDbContext : DbContext
    {
        public ProgressDbContext(DbContextOptions<ProgressDbContext> options) : base(options) { }

        public DbSet<VideoProgress> VideoProgresses { get; set; }
        public DbSet<CourseProgress> CourseProgresses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VideoProgress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.VideoId }).IsUnique();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.VideoId).IsRequired();
                entity.Property(e => e.CurrentTimeSeconds).IsRequired();
                entity.Property(e => e.CompletionPercentage).IsRequired();
                entity.Property(e => e.IsCompleted).IsRequired();
                entity.Property(e => e.LastWatched).IsRequired();
            });

            modelBuilder.Entity<CourseProgress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.CourseId).IsRequired();
                entity.Property(e => e.CompletedVideos).IsRequired();
                entity.Property(e => e.TotalVideos).IsRequired();
                entity.Property(e => e.CompletionPercentage).IsRequired();
                entity.Property(e => e.TotalWatchTimeSeconds).IsRequired();
                entity.Property(e => e.LastAccessed).IsRequired();
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.CourseId).IsRequired();
                entity.Property(e => e.EnrollmentDate).IsRequired();
                entity.Property(e => e.Status).HasConversion<string>().IsRequired();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.Details).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
            });
        }
    }
}