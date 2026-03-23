using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkloadManager.Models;
using WorkloadManager.Services;

namespace WorkloadManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly JobService _jobService;

        public JobsController(JobService jobService)
        {
            _jobService = jobService;
        }

        /// <summary>
        /// Get list of available job types with their specifications
        /// </summary>
        [HttpGet("types")]
        public IActionResult GetJobTypes()
        {
            var jobTypes = new[]
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

            return Ok(jobTypes);
        }

        /// <summary>
        /// Create a new job with detailed parameters
        /// </summary>
        [HttpPost("create")]
        public IActionResult CreateJob([FromBody] JobCreateRequest request)
        {
            try
            {
                var builder = new JobBuilder(request.Type, request.Name)
                    .WithDescription(request.Description)
                    .WithPayload(request.Payload)
                    .WithPriority(request.Priority)
                    .WithMaxAttempts(request.MaxAttempts);

                if (request.ScheduledFor.HasValue)
                {
                    builder.WithScheduledTime(request.ScheduledFor.Value);
                }

                if (!string.IsNullOrEmpty(request.IdempotencyKey))
                {
                    builder.WithIdempotencyKey(request.IdempotencyKey);
                }

                var job = builder.Build();

                return Ok(new
                {
                    Message = "Job created successfully",
                    Job = job
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Execute a job with the specified type
        /// </summary>
        [HttpPost("execute/{jobType}")]
        public async Task<IActionResult> ExecuteJob(string jobType, [FromQuery] int priority = 5, [FromQuery] int maxAttempts = 3)
        {
            try
            {
                var job = new JobBuilder(jobType, $"{jobType} Job")
                    .WithPriority(priority)
                    .WithMaxAttempts(maxAttempts)
                    .Build();

                job.Status = JobStatus.Processing;
                job.StartedAt = DateTime.UtcNow;

                var result = await _jobService.ExecuteJobAsync(job);

                job.Status = result.Success ? JobStatus.Completed : JobStatus.Failed;
                job.CompletedAt = DateTime.UtcNow;
                job.UpdatedAt = DateTime.UtcNow;

                if (!result.Success)
                {
                    job.ErrorMessage = result.Message;
                    job.CurrentAttempt++;
                }

                if (result.Data is Dictionary<string, object> dataDict && dataDict.ContainsKey("CompletionPercentage"))
                {
                    job.ProgressPercentage = (int)dataDict["CompletionPercentage"];
                }

                return Ok(new
                {
                    Job = new
                    {
                        job.Id,
                        job.Type,
                        job.Name,
                        job.Status,
                        job.Priority,
                        job.CurrentAttempt,
                        job.MaxAttempts,
                        job.ProgressPercentage,
                        job.CreatedAt,
                        job.StartedAt,
                        job.CompletedAt,
                        job.IdempotencyKey
                    },
                    Result = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Execute a custom job
        /// </summary>
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteCustomJob([FromBody] Job job)
        {
            if (string.IsNullOrEmpty(job.Type))
            {
                return BadRequest(new { error = "Job type is required" });
            }

            try
            {
                job.CreatedAt = DateTime.UtcNow;
                job.UpdatedAt = DateTime.UtcNow;
                job.Status = JobStatus.Processing;
                job.StartedAt = DateTime.UtcNow;

                var result = await _jobService.ExecuteJobAsync(job);

                job.Status = result.Success ? JobStatus.Completed : JobStatus.Failed;
                job.CompletedAt = DateTime.UtcNow;
                job.UpdatedAt = DateTime.UtcNow;

                if (!result.Success)
                {
                    job.ErrorMessage = result.Message;
                    job.CurrentAttempt++;
                }

                return Ok(new
                {
                    Job = new
                    {
                        job.Id,
                        job.Type,
                        job.Name,
                        job.Status,
                        job.Priority,
                        job.CurrentAttempt,
                        job.MaxAttempts,
                        job.ProgressPercentage,
                        job.CreatedAt,
                        job.StartedAt,
                        job.CompletedAt,
                        job.IdempotencyKey,
                        job.ErrorMessage
                    },
                    Result = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test all job types with various priorities and attempt configurations
        /// </summary>
        [HttpPost("test-all")]
        public async Task<IActionResult> TestAllJobTypes()
        {
            var results = new List<object>();
            var jobTypes = new[] { "email", "webhook", "report", "batch" };
            var priorities = new[] { 3, 5, 7, 10 }; // Different priorities for each job type

            for (int i = 0; i < jobTypes.Length; i++)
            {
                var jobType = jobTypes[i];
                var priority = priorities[i];

                var job = new JobBuilder(jobType, $"{jobType} Test Job")
                    .WithDescription($"Test job for {jobType} with priority {priority}")
                    .WithPriority(priority)
                    .WithMaxAttempts(3)
                    .Build();

                job.Status = JobStatus.Processing;
                job.StartedAt = DateTime.UtcNow;

                var result = await _jobService.ExecuteJobAsync(job);

                job.Status = result.Success ? JobStatus.Completed : JobStatus.Failed;
                job.CompletedAt = DateTime.UtcNow;
                job.UpdatedAt = DateTime.UtcNow;

                if (!result.Success)
                {
                    job.ErrorMessage = result.Message;
                    job.CurrentAttempt++;
                }

                results.Add(new
                {
                    Job = new
                    {
                        job.Id,
                        job.Type,
                        job.Name,
                        job.Status,
                        job.Priority,
                        job.CurrentAttempt,
                        job.MaxAttempts,
                        job.CreatedAt,
                        job.StartedAt,
                        job.CompletedAt,
                        job.IdempotencyKey
                    },
                    Result = result,
                    ExecutionTimeMs = (job.CompletedAt - job.StartedAt)?.TotalMilliseconds
                });
            }

            return Ok(new
            {
                TotalJobs = results.Count,
                Jobs = results
            });
        }

        /// <summary>
        /// Get job status information
        /// </summary>
        [HttpGet("statuses")]
        public IActionResult GetJobStatuses()
        {
            var statuses = Enum.GetValues(typeof(JobStatus))
                .Cast<JobStatus>()
                .Select(s => new { Value = (int)s, Name = s.ToString() })
                .ToList();

            return Ok(statuses);
        }

        /// <summary>
        /// Simulate retry logic for a failed job
        /// </summary>
        [HttpPost("retry/{jobId}")]
        public async Task<IActionResult> RetryJob(int jobId)
        {
            try
            {
                // In a real scenario, you'd fetch this from a database
                var job = new JobBuilder("webhook", "Retry Job")
                    .WithMaxAttempts(3)
                    .Build();

                job.Id = jobId;
                job.CurrentAttempt++;

                if (job.CurrentAttempt > job.MaxAttempts)
                {
                    return BadRequest(new
                    {
                        error = "Max retry attempts exceeded",
                        currentAttempt = job.CurrentAttempt,
                        maxAttempts = job.MaxAttempts
                    });
                }

                job.Status = JobStatus.Processing;
                job.StartedAt = DateTime.UtcNow;

                var result = await _jobService.ExecuteJobAsync(job);

                job.Status = result.Success ? JobStatus.Completed : JobStatus.Failed;
                job.CompletedAt = DateTime.UtcNow;

                return Ok(new
                {
                    Job = new
                    {
                        job.Id,
                        job.CurrentAttempt,
                        job.MaxAttempts,
                        job.Status
                    },
                    Result = result,
                    CanRetry = job.CurrentAttempt < job.MaxAttempts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

