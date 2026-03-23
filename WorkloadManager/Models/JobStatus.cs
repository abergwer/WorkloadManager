namespace WorkloadManager.Models
{
    public enum JobStatus
    {
        Scheduled = 0,
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5
    }
}
