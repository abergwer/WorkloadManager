using Microsoft.Extensions.Hosting;

namespace WorkloadManager.Services.BackgroundService
{
    public class BackgroundWorker : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly string _instanceId;

        public BackgroundWorker()
        { 
            _instanceId = Guid.NewGuid().ToString();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
