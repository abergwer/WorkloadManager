namespace WorkloadManager.Models
{
    public class JobCreateRequest
    {
        /// <summary>
        /// Type of job (email, webhook, report, batch, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Job name/title
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Job description (optional)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Input payload as JSON string (optional)
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// Priority level (0-10). Default is 5.
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Maximum number of retry attempts. Default is 3.
        /// </summary>
        public int MaxAttempts { get; set; } = 3;

        /// <summary>
        /// Optional scheduled execution time
        /// </summary>
        public DateTime? ScheduledFor { get; set; }

        /// <summary>
        /// Optional idempotency key for duplicate prevention
        /// </summary>
        public string IdempotencyKey { get; set; }
    }
}
