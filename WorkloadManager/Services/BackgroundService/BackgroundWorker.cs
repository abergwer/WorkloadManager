using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WorkloadManager.Models;
using WorkloadManager.Services;
using WorkloadManager.Database;

namespace WorkloadManager.Services.BackgroundService
{
    public class BackgroundWorker : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly string _instanceId;
        private readonly IServiceProvider _serviceProvider;

        public BackgroundWorker(IServiceProvider serviceProvider)
        { 
            _serviceProvider = serviceProvider;
            _instanceId = Guid.NewGuid().ToString();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Job Scheduler Service is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobService = scope.ServiceProvider.GetRequiredService<JobService>();
                    var repository = scope.ServiceProvider.GetRequiredService<WorkloadRepository>();
                    var jobs = await jobService.GetAllJobsAsync(cancellationToken: stoppingToken);
                    var now = DateTime.UtcNow;

                    // Get jobs that are ready to execute (Scheduled or Pending, and scheduled time has passed)
                    var jobsToConsider = jobs.Where(j =>
                        (j.Status == JobStatus.Scheduled || j.Status == JobStatus.Pending) &&
                        (j.ScheduledFor == null || j.ScheduledFor <= now)
                    ).ToList();

                    // Process each job with atomic claiming to prevent duplicate execution
                    foreach (var job in jobsToConsider)
                    {
                        try
                        {
                            // Atomically claim the job - only succeeds for one instance
                            var claimedJob = await repository.TryClaimJobForExecutionAsync(job.Id, stoppingToken);
                            
                            if (claimedJob == null)
                            {
                                // Another instance already claimed this job
                                Console.WriteLine($"Job {job.Id} was claimed by another instance, skipping");
                                continue;
                            }

                            Console.WriteLine($"Executing job {claimedJob.Id}: {claimedJob.Name}");
                            var (updatedJob, result) = await jobService.ExecuteJobWithTrackingAsync(claimedJob, stoppingToken);
                            Console.WriteLine($"Job {claimedJob.Id} completed with status: {updatedJob.Status}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error executing job {job.Id}: {ex.Message}");
                        }
                    }
                }
                
                // Add delay to prevent tight loop
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            Console.WriteLine("Job Scheduler Service is stopping.");
        }
    }
}
