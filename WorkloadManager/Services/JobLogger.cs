using WorkloadManager.Models;
using System.Text.Json;

namespace WorkloadManager.Services
{
    
    public class JobLogger
    {
        public static void LogInfo(Job job, string message, Dictionary<string, object> metadata = null)
        {
            AddLog(job, WorkloadManager.Models.LogLevel.Info , message, metadata);
        }

        public static void LogWarning(Job job, string message, Dictionary<string, object> metadata = null)
        {
            AddLog(job, WorkloadManager.Models.LogLevel.Warning, message, metadata);
        }

        public static void LogError(Job job, string message, Exception exception = null, Dictionary<string, object> metadata = null)
        {
            if (metadata == null)
                metadata = new Dictionary<string, object>();

            if (exception != null)
            {
                metadata["ExceptionType"] = exception.GetType().Name;
                metadata["ExceptionMessage"] = exception.Message;
            }

            AddLog(job, WorkloadManager.Models.LogLevel.Error, message, metadata);
        }

        private static void AddLog(Job job, WorkloadManager.Models.LogLevel level, string message, Dictionary<string, object> metadata = null)
        {
            var log = new JobLog
            {
                JobId = job.Id,
                Level = level,
                Message = message,
                Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null,
                Timestamp = DateTime.UtcNow
            };

            job.Logs.Add(log);
        }
    }
}
