using WorkloadManager.Models;

namespace WorkloadManager.Services
{
    public class BatchJobExecutor : IJobExecutor
    {
        private static readonly Random _random = new Random();

        public async Task<JobResult> ExecuteAsync(Job job, CancellationToken cancellationToken = default)
        {
            // Process multiple items with progress tracking
            int itemCount = _random.Next(5, 16); // 5-15 items
            var processedItems = new List<object>();

            for (int i = 1; i <= itemCount; i++)
            {
                // Simulate processing each item with small delay (100-300ms)
                int delayMs = _random.Next(100, 301);
                await Task.Delay(delayMs, cancellationToken);

                int progressPercentage = (int)((i / (double)itemCount) * 100);

                processedItems.Add(new
                {
                    ItemId = i,
                    Status = "Processed",
                    ProcessedAt = DateTime.UtcNow
                });
            }

            return new JobResult
            {
                Success = true,
                Message = $"Batch job completed - processed {itemCount} items",
                Data = new
                {
                    TotalItems = itemCount,
                    ProcessedItems = itemCount,
                    FailedItems = 0,
                    CompletionPercentage = 100,
                    Items = processedItems
                },
                Metadata = new Dictionary<string, object>
                {
                    { "BatchId", Guid.NewGuid().ToString().Substring(0, 8).ToUpper() },
                    { "TotalItems", itemCount },
                    { "SuccessCount", itemCount },
                    { "FailureCount", 0 },
                    { "CompletionPercentage", 100 }
                }
            };
        }
    }
}
