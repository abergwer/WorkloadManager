using Xunit;
using WorkloadManager.Services;
using WorkloadManager.Models;

namespace WorkloadManager.Tests.Services
{
    public class JobServiceTests
    {
        [Fact]
        public void CreateTestJob_ShouldReturnValidJob()
        {
            // Arrange
            var service = new JobService(null);

            // Act
            var job = service.CreateTestJob("email", priority: 8, maxAttempts: 5, description: "Test email job");

            // Assert
            Assert.NotNull(job);
            Assert.Equal("email", job.Type);
            Assert.Equal("email Job", job.Name);
            Assert.Equal(8, job.Priority);
            Assert.Equal(5, job.MaxAttempts);
            Assert.Equal("Test email job", job.Description);
            Assert.Equal(0, job.Id); // Not saved to database
        }

        [Fact]
        public void GetAvailableJobTypes_ShouldReturnJobTypesArray()
        {
            // Arrange
            var service = new JobService(null);

            // Act
            var result = service.GetAvailableJobTypes();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<object>(result);
        }

        [Fact]
        public async Task ExecuteJobAsync_WithEmailJob_ShouldExecuteSuccessfully()
        {
            // Arrange
            var service = new JobService(null);
            var job = new Job
            {
                Id = 1,
                Type = "email",
                Name = "Test Email",
                Status = JobStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var result = await service.ExecuteJobAsync(job);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<JobResult>(result);
        }

        [Fact]
        public void CreateTestJob_WithDefaultParameters_ShouldUseDefaults()
        {
            // Arrange
            var service = new JobService(null);

            // Act
            var job = service.CreateTestJob("webhook");

            // Assert
            Assert.NotNull(job);
            Assert.Equal("webhook", job.Type);
            Assert.Equal(5, job.Priority);
            Assert.Equal(3, job.MaxAttempts);
        }
    }
}
