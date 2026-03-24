using WorkloadManager.Models;

namespace WorkloadManager.Services.Executors
{
    public class ReportJobExecutor : IJobExecutor
    {
        private static readonly Random _random = new Random();

        public async Task<JobResult> ExecuteAsync(Job job, CancellationToken cancellationToken = default)
        {
            // Simulate report generation delay (3-5 seconds)
            int delayMs = _random.Next(3000, 5001);
            await Task.Delay(delayMs, cancellationToken);

            // Generate mock file URL
            string reportId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string fileUrl = $"https://reports.example.com/files/report-{reportId}.pdf";

            return new JobResult
            {
                Success = true,
                Message = "Report generated successfully",
                Data = new
                {
                    FileUrl = fileUrl,
                    FileName = $"report-{reportId}.pdf",
                    FileSize = "2.5 MB",
                    Format = "PDF",
                    GeneratedAt = DateTime.UtcNow
                },
                Metadata = new Dictionary<string, object>
                {
                    { "ReportId", reportId },
                    { "RowsProcessed", _random.Next(100, 10000) },
                    { "ProcessingTimeMs", delayMs }
                }
            };
        }
    }
}
