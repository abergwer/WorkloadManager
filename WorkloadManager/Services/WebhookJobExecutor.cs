using WorkloadManager.Models;

namespace WorkloadManager.Services
{
    public class WebhookJobExecutor : IJobExecutor
    {
        private static readonly Random _random = new Random();

        public async Task<JobResult> ExecuteAsync(Job job, CancellationToken cancellationToken = default)
        {
            // Simulate webhook call delay (1-2 seconds)
            int delayMs = _random.Next(1000, 2001);
            await Task.Delay(delayMs, cancellationToken);

            // 80% success rate, 20% failure
            bool success = _random.NextDouble() < 0.8;

            if (success)
            {
                return new JobResult
                {
                    Success = true,
                    Message = "Webhook call completed successfully",
                    Data = new
                    {
                        WebhookUrl = "https://api.example.com/webhook",
                        ResponseCode = 200,
                        ResponseTime = $"{delayMs}ms"
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        { "Attempt", 1 },
                        { "ProcessingTimeMs", delayMs }
                    }
                };
            }
            else
            {
                return new JobResult
                {
                    Success = false,
                    Message = "Webhook call failed - simulated failure for retry testing",
                    Data = new
                    {
                        WebhookUrl = "https://api.example.com/webhook",
                        ResponseCode = 503,
                        Error = "Service Unavailable"
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        { "Attempt", 1 },
                        { "Retryable", true },
                        { "ProcessingTimeMs", delayMs }
                    }
                };
            }
        }
    }
}
