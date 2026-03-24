using WorkloadManager.Models;

namespace WorkloadManager.Services.Executors
{
    public interface IJobExecutor
    {
        Task<JobResult> ExecuteAsync(Job job, CancellationToken cancellationToken = default);
    }
}
