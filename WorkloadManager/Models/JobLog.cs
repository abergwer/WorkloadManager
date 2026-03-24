namespace WorkloadManager.Models
{
    public class JobLog
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to the Job
        /// </summary>
        public int JobId { get; set; }
        
        /// <summary>
        /// Navigation property to Job (optional for JSON serialization)
        /// </summary>
        public Job? Job { get; set; }
        
        /// <summary>
        /// Log level: Info, Warning, Error
        /// </summary>
        public LogLevel Level { get; set; }
        
        /// <summary>
        /// Log message
        /// </summary>
        public required string Message { get; set; }
        
        /// <summary>
        /// Additional metadata as JSON
        /// </summary>
        public string? Metadata { get; set; }
        
        /// <summary>
        /// Timestamp when the log entry was created
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
