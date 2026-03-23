using WorkloadManager.Models;

namespace WorkloadManager.Services
{
    public class JobService
    {
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
    }
}
