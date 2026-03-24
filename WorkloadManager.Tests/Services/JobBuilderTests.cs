using Xunit;
using WorkloadManager.Services;
using WorkloadManager.Models;

namespace WorkloadManager.Tests.Services
{
    public class JobBuilderTests
    {
        [Fact]
        public void Build_WithMinimalParameters_ShouldCreateValidJob()
        {
            // Arrange & Act
            var job = new JobBuilder("email", "Test Email Job")
                .Build();

            // Assert
            Assert.NotNull(job);
            Assert.Equal("email", job.Type);
            Assert.Equal("Test Email Job", job.Name);
            Assert.Equal(JobStatus.Pending, job.Status);
            Assert.Equal(5, job.Priority);
            Assert.Equal(3, job.MaxAttempts);
            Assert.Equal(0, job.ProgressPercentage);
        }

        [Fact]
        public void Build_WithDescription_ShouldSetDescription()
        {
            // Arrange & Act
            var job = new JobBuilder("email", "Test Email Job")
                .WithDescription("This is a test job")
                .Build();

            // Assert
            Assert.Equal("This is a test job", job.Description);
        }

        [Fact]
        public void Build_WithPayload_ShouldSetPayload()
        {
            // Arrange
            var payload = "{\"email\": \"test@example.com\"}";

            // Act
            var job = new JobBuilder("email", "Test Email Job")
                .WithPayload(payload)
                .Build();

            // Assert
            Assert.Equal(payload, job.Payload);
        }

        [Fact]
        public void Build_WithPriority_ShouldSetPriority()
        {
            // Arrange & Act
            var job = new JobBuilder("email", "Test Email Job")
                .WithPriority(9)
                .Build();

            // Assert
            Assert.Equal(9, job.Priority);
        }

        [Fact]
        public void Build_WithMaxAttempts_ShouldSetMaxAttempts()
        {
            // Arrange & Act
            var job = new JobBuilder("email", "Test Email Job")
                .WithMaxAttempts(5)
                .Build();

            // Assert
            Assert.Equal(5, job.MaxAttempts);
        }

        [Fact]
        public void Build_WithScheduledTime_ShouldSetStatusToScheduled()
        {
            // Arrange
            var scheduledTime = DateTime.UtcNow.AddHours(1);

            // Act
            var job = new JobBuilder("email", "Test Email Job")
                .WithScheduledTime(scheduledTime)
                .Build();

            // Assert
            Assert.Equal(JobStatus.Scheduled, job.Status);
            Assert.Equal(scheduledTime, job.ScheduledFor);
        }

        [Fact]
        public void Build_WithPastScheduledTime_ShouldSetStatusToPending()
        {
            // Arrange
            var pastTime = DateTime.UtcNow.AddHours(-1);

            // Act
            var job = new JobBuilder("email", "Test Email Job")
                .WithScheduledTime(pastTime)
                .Build();

            // Assert
            Assert.Equal(JobStatus.Pending, job.Status);
        }

        [Fact]
        public void Build_WithChainedMethods_ShouldApplyAllSettings()
        {
            // Arrange
            var scheduledTime = DateTime.UtcNow.AddHours(1);
            var payload = "{\"key\": \"value\"}";

            // Act
            var job = new JobBuilder("webhook", "Test Webhook Job")
                .WithDescription("Webhook test job")
                .WithPayload(payload)
                .WithPriority(8)
                .WithMaxAttempts(4)
                .WithScheduledTime(scheduledTime)
                .Build();

            // Assert
            Assert.Equal("webhook", job.Type);
            Assert.Equal("Test Webhook Job", job.Name);
            Assert.Equal("Webhook test job", job.Description);
            Assert.Equal(payload, job.Payload);
            Assert.Equal(8, job.Priority);
            Assert.Equal(4, job.MaxAttempts);
            Assert.Equal(JobStatus.Scheduled, job.Status);
            Assert.Equal(scheduledTime, job.ScheduledFor);
        }

        [Fact]
        public void Build_ShouldSetCreatedAtAndUpdatedAt()
        {
            // Arrange & Act
            var beforeBuild = DateTime.UtcNow;
            var job = new JobBuilder("email", "Test Email Job").Build();
            var afterBuild = DateTime.UtcNow;

            // Assert
            Assert.True(job.CreatedAt >= beforeBuild && job.CreatedAt <= afterBuild);
            Assert.True(job.UpdatedAt >= beforeBuild && job.UpdatedAt <= afterBuild);
        }
    }
}
