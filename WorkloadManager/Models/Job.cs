namespace WorkloadManager.Models
{
    public class Job
    {
        /// <summary>
        /// Unique identifier for the job
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Type of job (email, webhook, report, batch, etc.)
        /// </summary>
        public required string Type { get; set; }
        
        /// <summary>
        /// Current status of the job
        /// </summary>
        public JobStatus Status { get; set; }
        
        /// <summary>
        /// Job name/title
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Job description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Input payload as JSON string
        /// </summary>
        public string? Payload { get; set; }
        
        /// <summary>
        /// Result/output as JSON string (populated after execution)
        /// </summary>
        public string? Result { get; set; }
        
        /// <summary>
        /// Priority level (0 = lowest, 10 = highest). Default is 5.
        /// </summary>
        public int Priority { get; set; } = 5;
        
        /// <summary>
        /// Current attempt number (starts at 0)
        /// </summary>
        public int CurrentAttempt { get; set; } = 0;
        
        /// <summary>
        /// Maximum number of attempts allowed
        /// </summary>
        public int MaxAttempts { get; set; } = 3;
        
        /// <summary>
        /// Error message from last failed attempt (if applicable)
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Error details/stack trace (if applicable)
        /// </summary>
        public string? ErrorDetails { get; set; }
        
        /// <summary>
        /// Progress percentage for long-running jobs (0-100)
        /// </summary>
        public int ProgressPercentage { get; set; } = 0;
        
        /// <summary>
        /// Scheduled execution time (null = execute immediately)
        /// </summary>
        public DateTime? ScheduledFor { get; set; }
        
        /// <summary>
        /// Timestamp when the job was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Timestamp when the job execution started
        /// </summary>
        public DateTime? StartedAt { get; set; }
        
        /// <summary>
        /// Timestamp when the job completed (successfully or with failure)
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// Timestamp when the job was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Navigation property for associated logs
        /// </summary>
        public ICollection<JobLog> Logs { get; set; } = new List<JobLog>();
    }
}
