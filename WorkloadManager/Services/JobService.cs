using WorkloadManager.Database;
using WorkloadManager.Models;
using WorkloadManager.Services.Executors;

namespace WorkloadManager.Services
{
    public class JobService
    {
        private readonly WorkloadRepository _repository;

        public JobService(WorkloadRepository repository)
        {
            _repository = repository;
        }

        public async Task<JobResult> ExecuteJobAsync(Job job, CancellationToken cancellationToken = default)
        {
            try
            {
                var executor = JobExecutorFactory.CreateExecutor(job.Type);
                return await executor.ExecuteAsync(job, cancellationToken);
            }
            catch (Exception ex)
            {
                return new JobResult
                {
                    Success = false,
                    Message = $"Job execution failed: {ex.Message}",
                    Data = null,
                    Metadata = new Dictionary<string, object>
                    {
                        { "ErrorType", ex.GetType().Name }
                    }
                };
            }
        }

        /// <summary>
        /// Execute a job and automatically track status and timestamps, then save to database
        /// </summary>
        public async Task<(Job job, JobResult result)> ExecuteJobWithTrackingAsync(Job job, CancellationToken cancellationToken = default)
        {
            // Save job as processing
            job.Status = JobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            
            if (job.Id == 0)
            {
                job = await _repository.CreateJobAsync(job, cancellationToken);
            }
            else
            {
                job = await _repository.UpdateJobAsync(job, cancellationToken);
            }

            var result = await ExecuteJobAsync(job, cancellationToken);

            job.Status = result.Success ? JobStatus.Completed : JobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;

            if (!result.Success)
            {
                job.ErrorMessage = result.Message;
                job.CurrentAttempt++;
            }

            // Update progress if included in result
            if (result.Data is Dictionary<string, object> dataDict && dataDict.ContainsKey("CompletionPercentage"))
            {
                job.ProgressPercentage = (int)dataDict["CompletionPercentage"];
            }

            // Save final job state
            job = await _repository.UpdateJobAsync(job, cancellationToken);

            return (job, result);
        }

        /// <summary>
        /// Create a job from a creation request and save to database
        /// </summary>
        public async Task<Job> CreateJobFromRequestAsync(JobCreateRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Type))
                throw new ArgumentException("Job type is required", nameof(request));
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Job name is required", nameof(request));

            var builder = new JobBuilder(request.Type, request.Name)
                .WithDescription(request.Description)
                .WithPayload(request.Payload)
                .WithPriority(request.Priority)
                .WithMaxAttempts(request.MaxAttempts);

            if (request.ScheduledFor.HasValue)
            {
                builder.WithScheduledTime(request.ScheduledFor.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                builder.WithIdempotencyKey(request.IdempotencyKey);
            }

            var job = builder.Build();
            return await _repository.CreateJobAsync(job, cancellationToken);
        }

        /// <summary>
        /// Create a quick test job with given parameters (not saved to database)
        /// </summary>
        public Job CreateTestJob(string jobType, int priority = 5, int maxAttempts = 3, string description = null)
        {
            var builder = new JobBuilder(jobType, $"{jobType} Job")
                .WithPriority(priority)
                .WithMaxAttempts(maxAttempts);

            if (!string.IsNullOrEmpty(description))
            {
                builder.WithDescription(description);
            }

            return builder.Build();
        }

        /// <summary>
        /// Get all available job type specifications
        /// </summary>
        public object GetAvailableJobTypes()
        {
            return new[]
            {
                new
                {
                    Type = "email",
                    Description = "Simulates sending an email",
                    Processing = "1-3 seconds",
                    Returns = "Success with mock message ID"
                },
                new
                {
                    Type = "webhook",
                    Description = "Simulates calling an external webhook",
                    Processing = "1-2 seconds",
                    Returns = "80% success, 20% simulated failure"
                },
                new
                {
                    Type = "report",
                    Description = "Simulates generating a report",
                    Processing = "3-5 seconds",
                    Returns = "Mock file URL"
                },
                new
                {
                    Type = "batch",
                    Description = "Processes multiple items with progress tracking",
                    Processing = "Multiple items with delays",
                    Returns = "Summary with processed items"
                }
            };
        }

        /// <summary>
        /// Get all job statuses as a list
        /// </summary>
        public List<object> GetJobStatuses()
        {
            return Enum.GetValues<JobStatus>()
                .Select(s => new { Value = (int)s, Name = s.ToString() })
                .Cast<object>()
                .ToList();
        }

        /// <summary>
        /// Execute all job types for testing
        /// </summary>
        public async Task<List<object>> ExecuteAllJobTypesAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<object>();
            var jobTypes = new[] { "email", "webhook", "report", "batch" };
            var priorities = new[] { 3, 5, 7, 10 };

            for (int i = 0; i < jobTypes.Length; i++)
            {
                var jobType = jobTypes[i];
                var priority = priorities[i];
                var description = $"Test job for {jobType} with priority {priority}";

                var job = CreateTestJob(jobType, priority, 3, description);
                var (trackedJob, result) = await ExecuteJobWithTrackingAsync(job, cancellationToken);

                results.Add(new
                {
                    Job = new
                    {
                        trackedJob.Id,
                        trackedJob.Type,
                        trackedJob.Name,
                        trackedJob.Status,
                        trackedJob.Priority,
                        trackedJob.CurrentAttempt,
                        trackedJob.MaxAttempts,
                        trackedJob.CreatedAt,
                        trackedJob.StartedAt,
                        trackedJob.CompletedAt,
                        trackedJob.IdempotencyKey
                    },
                    Result = result,
                    ExecutionTimeMs = (trackedJob.CompletedAt - trackedJob.StartedAt)?.TotalMilliseconds
                });
            }

            return results;
        }

        /// <summary>
        /// Retry a failed job
        /// </summary>
        public async Task<(bool canRetry, object retryResult)> RetryJobAsync(Job job, CancellationToken cancellationToken = default)
        {
            job.CurrentAttempt++;

            if (job.CurrentAttempt > job.MaxAttempts)
            {
                return (false, new
                {
                    error = "Max retry attempts exceeded",
                    currentAttempt = job.CurrentAttempt,
                    maxAttempts = job.MaxAttempts
                });
            }

            var (trackedJob, result) = await ExecuteJobWithTrackingAsync(job, cancellationToken);

            return (trackedJob.CurrentAttempt < trackedJob.MaxAttempts, new
            {
                Job = new
                {
                    trackedJob.Id,
                    trackedJob.CurrentAttempt,
                    trackedJob.MaxAttempts,
                    trackedJob.Status
                },
                Result = result,
                CanRetry = trackedJob.CurrentAttempt < trackedJob.MaxAttempts
            });
        }

        // ========== Repository Query Methods ==========

        /// <summary>
        /// Get a job by ID from database
        /// </summary>
        public async Task<Job?> GetJobByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetJobByIdAsync(id, cancellationToken);
        }

        /// <summary>
        /// Get all jobs with pagination
        /// </summary>
        public async Task<List<Job>> GetAllJobsAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
        {
            return await _repository.GetJobsAsync(skip, take, cancellationToken);
        }

        /// <summary>
        /// Get jobs filtered by status
        /// </summary>
        public async Task<List<Job>> GetJobsByStatusAsync(JobStatus status, CancellationToken cancellationToken = default)
        {
            return await _repository.GetJobsByStatusAsync(status, cancellationToken);
        }

        /// <summary>
        /// Get jobs filtered by type
        /// </summary>
        public async Task<List<Job>> GetJobsByTypeAsync(string type, CancellationToken cancellationToken = default)
        {
            return await _repository.GetJobsByTypeAsync(type, cancellationToken);
        }

        /// <summary>
        /// Get job statistics
        /// </summary>
        public async Task<object> GetJobStatisticsAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetJobStatisticsAsync(cancellationToken);
        }
    }
}

