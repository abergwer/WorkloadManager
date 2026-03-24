using WorkloadManager.Models;

namespace WorkloadManager.Services
{
    public class JobBuilder
    {
        private readonly Job _job;

        public JobBuilder(string type, string name)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Job type cannot be empty", nameof(type));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Job name cannot be empty", nameof(name));

            _job = new Job
            {
                Type = type,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = JobStatus.Pending,
                Priority = 5,
                MaxAttempts = 3
            };
        }

        public JobBuilder WithDescription(string? description)
        {
            if (!string.IsNullOrWhiteSpace(description))
                _job.Description = description;
            return this;
        }

        public JobBuilder WithPayload(string? payload)
        {
            if (!string.IsNullOrWhiteSpace(payload))
                _job.Payload = payload;
            return this;
        }

        public JobBuilder WithPriority(int priority)
        {
            _job.Priority = Math.Max(0, Math.Min(10, priority)); // Clamp 0-10
            return this;
        }

        public JobBuilder WithMaxAttempts(int maxAttempts)
        {
            _job.MaxAttempts = Math.Max(1, maxAttempts);
            return this;
        }

        public JobBuilder WithScheduledTime(DateTime scheduledFor)
        {
            _job.ScheduledFor = scheduledFor;
            if (scheduledFor > DateTime.UtcNow)
            {
                _job.Status = JobStatus.Scheduled;
            }
            return this;
        }

        public Job Build()
        {
            return _job;
        }
    }
}
