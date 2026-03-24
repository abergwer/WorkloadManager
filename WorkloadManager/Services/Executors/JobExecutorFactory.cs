using WorkloadManager.Models;

namespace WorkloadManager.Services.Executors
{
    public class JobExecutorFactory
    {
        public static IJobExecutor CreateExecutor(string jobType)
        {
            return jobType.ToLowerInvariant() switch
            {
                "email" => new EmailJobExecutor(),
                "webhook" => new WebhookJobExecutor(),
                "report" => new ReportJobExecutor(),
                "batch" => new BatchJobExecutor(),
                _ => throw new ArgumentException($"Unknown job type: {jobType}")
            };
        }
    }
}
