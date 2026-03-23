using WorkloadManager.Models;

namespace WorkloadManager.Services
{
    public interface IJobExecutor
    {
        Task<JobResult> ExecuteAsync(Job job, CancellationToken cancellationToken = default);
    }
}
