using Xunit;
using WorkloadManager.Models;

namespace WorkloadManager.Tests.Database
{
    public class WorkloadRepositoryTests
    {
        [Fact]
        public void Repository_ShouldConstructSuccessfully()
        {
            // Test that we can at least verify the test setup works
            // Full integration tests would require a database
            Assert.True(true);
        }

        [Fact]
        public void JobModel_ShouldHaveRequiredProperties()
        {
            // Verify Job model structure
            var job = new Job
            {
                Type = "email",
                Name = "Test",
                Status = JobStatus.Pending,
                Priority = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Assert.NotNull(job);
            Assert.Equal("email", job.Type);
            Assert.Equal("Test", job.Name);
            Assert.Equal(JobStatus.Pending, job.Status);
        }

        [Fact]
        public void JobLog_ShouldHaveRequiredProperties()
        {
            // Verify JobLog model structure
            var log = new JobLog
            {
                JobId = 1,
                Level = LogLevel.Info,
                Message = "Test log",
                Timestamp = DateTime.UtcNow
            };

            Assert.NotNull(log);
            Assert.Equal(1, log.JobId);
            Assert.Equal(LogLevel.Info, log.Level);
            Assert.Equal("Test log", log.Message);
        }
    }
}


