using Microsoft.EntityFrameworkCore;
using WorkloadManager.Models;

namespace WorkloadManager.Database
{
    public class WorkloadRepository
    {
        private readonly WorkloadDbContext _context;

        public WorkloadRepository(WorkloadDbContext context)
        {
            _context = context;
        }

        // ========== Job Operations ==========

        /// <summary>
        /// Create and save a new job to the database
        /// </summary>
        public async Task<Job> CreateJobAsync(Job job, CancellationToken cancellationToken = default)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync(cancellationToken);
            return job;
        }

        /// <summary>
        /// Get a job by ID with its logs
        /// </summary>
        public async Task<Job?> GetJobByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Jobs
                .Include(j => j.Logs)
                .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
        }

        /// <summary>
        /// Get all jobs with pagination
        /// </summary>
        public async Task<List<Job>> GetJobsAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
        {
            return await _context.Jobs
                .Include(j => j.Logs)
                .OrderByDescending(j => j.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get jobs filtered by status
        /// </summary>
        public async Task<List<Job>> GetJobsByStatusAsync(JobStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Jobs
                .Include(j => j.Logs)
                .Where(j => j.Status == status)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get jobs filtered by type
        /// </summary>
        public async Task<List<Job>> GetJobsByTypeAsync(string type, CancellationToken cancellationToken = default)
        {
            return await _context.Jobs
                .Include(j => j.Logs)
                .Where(j => j.Type == type)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Update an existing job
        /// </summary>
        public async Task<Job> UpdateJobAsync(Job job, CancellationToken cancellationToken = default)
        {
            job.UpdatedAt = DateTime.UtcNow;
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync(cancellationToken);
            return job;
        }

        /// <summary>
        /// Delete a job by ID
        /// </summary>
        public async Task<bool> DeleteJobAsync(int id, CancellationToken cancellationToken = default)
        {
            var job = await GetJobByIdAsync(id, cancellationToken);
            if (job == null)
                return false;

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Get pending/scheduled jobs ready for execution
        /// </summary>
        public async Task<List<Job>> GetPendingJobsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Jobs
                .Include(j => j.Logs)
                .Where(j => (j.Status == JobStatus.Pending || j.Status == JobStatus.Scheduled) &&
                           (j.ScheduledFor == null || j.ScheduledFor <= DateTime.UtcNow))
                .OrderBy(j => j.Priority)
                .ThenBy(j => j.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get jobs that failed and can be retried
        /// </summary>
        public async Task<List<Job>> GetRetryableJobsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Jobs
                .Include(j => j.Logs)
                .Where(j => j.Status == JobStatus.Failed && j.CurrentAttempt < j.MaxAttempts)
                .OrderBy(j => j.Priority)
                .ThenBy(j => j.UpdatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get job statistics
        /// </summary>
        public async Task<object> GetJobStatisticsAsync(CancellationToken cancellationToken = default)
        {
            var stats = await _context.Jobs
                .GroupBy(j => j.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            var total = await _context.Jobs.CountAsync(cancellationToken);
            var byType = await _context.Jobs
                .GroupBy(j => j.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            return new
            {
                Total = total,
                ByStatus = stats,
                ByType = byType,
                RecentJobs = await _context.Jobs
                    .OrderByDescending(j => j.CreatedAt)
                    .Take(10)
                    .Select(j => new { j.Id, j.Type, j.Status, j.CreatedAt })
                    .ToListAsync(cancellationToken)
            };
        }

        // ========== JobLog Operations ==========

        /// <summary>
        /// Add a log entry to a job
        /// </summary>
        public async Task<JobLog> AddJobLogAsync(JobLog log, CancellationToken cancellationToken = default)
        {
            _context.JobLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
            return log;
        }

        /// <summary>
        /// Get logs for a specific job
        /// </summary>
        public async Task<List<JobLog>> GetJobLogsAsync(int jobId, CancellationToken cancellationToken = default)
        {
            return await _context.JobLogs
                .Where(l => l.JobId == jobId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get error logs for a specific job
        /// </summary>
        public async Task<List<JobLog>> GetJobErrorLogsAsync(int jobId, CancellationToken cancellationToken = default)
        {
            return await _context.JobLogs
                .Where(l => l.JobId == jobId && l.Level == WorkloadManager.Models.LogLevel.Error)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Delete old logs (for archival/cleanup)
        /// </summary>
        public async Task<int> DeleteOldLogsAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
        {
            var oldLogs = await _context.JobLogs
                .Where(l => l.Timestamp < beforeDate)
                .ToListAsync(cancellationToken);

            _context.JobLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync(cancellationToken);
            return oldLogs.Count;
        }

        /// <summary>
        /// Atomically claim a job for execution using a single UPDATE statement
        /// Only succeeds if job is in Scheduled or Pending status
        /// Returns the updated job if successfully claimed, null if already claimed by another instance
        /// </summary>
        public async Task<Job?> TryClaimJobForExecutionAsync(int jobId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            
            // Atomic UPDATE: only updates if status is Scheduled or Pending
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE Jobs 
                   SET Status = {(int)JobStatus.Processing}, StartedAt = {now}, UpdatedAt = {now}
                   WHERE Id = {jobId} 
                   AND (Status = {(int)JobStatus.Scheduled} OR Status = {(int)JobStatus.Pending})",
                cancellationToken);

            // If no rows were affected, another instance already claimed this job
            if (rowsAffected == 0)
                return null;

            // Fetch and return the updated job
            return await GetJobByIdAsync(jobId, cancellationToken);
        }
    }
}

