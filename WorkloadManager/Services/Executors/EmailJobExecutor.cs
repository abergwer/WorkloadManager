using WorkloadManager.Models;

namespace WorkloadManager.Services.Executors
{
    public class EmailJobExecutor : IJobExecutor
    {
        private static readonly Random _random = new Random();

        public async Task<JobResult> ExecuteAsync(Job job, CancellationToken cancellationToken = default)
        {
            // Simulate email send delay (1-3 seconds)
            int delayMs = _random.Next(1000, 3001);
            await Task.Delay(delayMs, cancellationToken);

            // Generate mock message ID
            string messageId = $"MSG-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            return new JobResult
            {
                Success = true,
                Message = "Email sent successfully",
                Data = new
                {
                    MessageId = messageId,
                    Recipient = "user@example.com",
                    Subject = "Test Email",
                    SentAt = DateTime.UtcNow
                },
                Metadata = new Dictionary<string, object>
                {
                    { "ProcessingTimeMs", delayMs }
                }
            };
        }
    }
}
