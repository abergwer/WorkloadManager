using Microsoft.EntityFrameworkCore;
using WorkloadManager.Models;

namespace WorkloadManager.Database
{
    public class WorkloadDbContext : DbContext
    {
        public WorkloadDbContext(DbContextOptions<WorkloadDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobLog> JobLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Job entity
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasDefaultValue(JobStatus.Pending);

                entity.Property(e => e.Priority)
                    .IsRequired()
                    .HasDefaultValue(5);

                entity.Property(e => e.CurrentAttempt)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.MaxAttempts)
                    .IsRequired()
                    .HasDefaultValue(3);

                entity.Property(e => e.ProgressPercentage)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.Payload)
                    .HasColumnType("text");

                entity.Property(e => e.Result)
                    .HasColumnType("text");

                entity.Property(e => e.ErrorMessage)
                    .HasMaxLength(1000);

                entity.Property(e => e.ErrorDetails)
                    .HasColumnType("text");

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.CreatedAt);

                // One-to-many relationship with JobLog
                entity.HasMany(e => e.Logs)
                    .WithOne(l => l.Job)
                    .HasForeignKey(l => l.JobId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure JobLog entity
            modelBuilder.Entity<JobLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.JobId)
                    .IsRequired();

                entity.Property(e => e.Level)
                    .IsRequired();

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Metadata)
                    .HasColumnType("text");

                entity.Property(e => e.Timestamp)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.Timestamp);
            });
        }
    }
}
