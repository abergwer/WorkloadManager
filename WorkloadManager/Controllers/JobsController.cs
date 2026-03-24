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
            var jobTypes = _jobService.GetAvailableJobTypes();
            return Ok(jobTypes);
        }

        /// <summary>
        /// Create a new job with detailed parameters
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateJob([FromBody] JobCreateRequest request)
        {
            // Validate request
            if (request == null)
            {
                return BadRequest(new { error = "Request body cannot be empty" });
            }

            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return BadRequest(new { error = "Job type is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { error = "Job name is required" });
            }

            try
            {
                var job = await _jobService.CreateJobFromRequestAsync(request);
                return CreatedAtAction(nameof(GetJob), new { id = job.Id }, new
                {
                    Message = "Job created successfully",
                    Job = job
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// Get a job by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            try
            {
                var job = await _jobService.GetJobByIdAsync(id);
                if (job == null)
                {
                    return NotFound(new { error = "Job not found" });
                }

                return Ok(job);
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
                var job = _jobService.CreateTestJob(jobType, priority, maxAttempts);
                var (trackedJob, result) = await _jobService.ExecuteJobWithTrackingAsync(job);

                return Ok(new
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
                        trackedJob.ProgressPercentage,
                        trackedJob.CreatedAt,
                        trackedJob.StartedAt,
                        trackedJob.CompletedAt,
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

                var (trackedJob, result) = await _jobService.ExecuteJobWithTrackingAsync(job);

                return Ok(new
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
                        trackedJob.ProgressPercentage,
                        trackedJob.CreatedAt,
                        trackedJob.StartedAt,
                        trackedJob.CompletedAt,
                        trackedJob.ErrorMessage
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
            try
            {
                var results = await _jobService.ExecuteAllJobTypesAsync();
                return Ok(new
                {
                    TotalJobs = results.Count,
                    Jobs = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get job status information
        /// </summary>
        [HttpGet("statuses")]
        public IActionResult GetJobStatuses()
        {
            var statuses = _jobService.GetJobStatuses();
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

                var (canRetry, retryResult) = await _jobService.RetryJobAsync(job);

                if (!canRetry && retryResult is Dictionary<string, object> errorDict && errorDict.ContainsKey("error"))
                {
                    return BadRequest(retryResult);
                }

                return Ok(retryResult);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

