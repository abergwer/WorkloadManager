using Xunit;
using WorkloadManager.Models;

namespace WorkloadManager.Tests.Controllers
{
    public class JobsControllerTests
    {
        [Fact]
        public void JobCreateRequest_WithValidData_ShouldHaveProperties()
        {
            // Arrange & Act
            var request = new JobCreateRequest
            {
                Type = "email",
                Name = "Test Job",
                Description = "Test Description",
                Priority = 7,
                MaxAttempts = 4,
                ScheduledFor = null,
                Payload = "{\"email\": \"test@example.com\"}"
            };

            // Assert
            Assert.NotNull(request);
            Assert.Equal("email", request.Type);
            Assert.Equal("Test Job", request.Name);
            Assert.Equal("Test Description", request.Description);
            Assert.Equal(7, request.Priority);
            Assert.Equal(4, request.MaxAttempts);
            Assert.Null(request.ScheduledFor);
        }

        [Fact]
        public void Job_WithValidData_ShouldHaveProperties()
        {
            // Arrange & Act
            var job = new Job
            {
                Id = 1,
                Type = "email",
                Name = "Test Job",
                Status = JobStatus.Pending,
                Priority = 5,
                MaxAttempts = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.NotNull(job);
            Assert.Equal(1, job.Id);
            Assert.Equal("email", job.Type);
            Assert.Equal(JobStatus.Pending, job.Status);
            Assert.Equal(5, job.Priority);
        }

        [Fact]
        public void JobStatus_ShouldHaveAllValues()
        {
            // Verify JobStatus enum has expected values
            Assert.True(Enum.IsDefined(typeof(JobStatus), JobStatus.Pending));
            Assert.True(Enum.IsDefined(typeof(JobStatus), JobStatus.Processing));
            Assert.True(Enum.IsDefined(typeof(JobStatus), JobStatus.Completed));
            Assert.True(Enum.IsDefined(typeof(JobStatus), JobStatus.Failed));
            Assert.True(Enum.IsDefined(typeof(JobStatus), JobStatus.Scheduled));
        }

        [Fact]
        public void LogLevel_ShouldHaveAllValues()
        {
            // Verify LogLevel enum has expected values
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Info));
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Warning));
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Error));
        }
    }
}

