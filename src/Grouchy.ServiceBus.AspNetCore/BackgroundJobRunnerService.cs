namespace Grouchy.ServiceBus.AspNetCore
{
   using System;
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;
   using Microsoft.Extensions.Hosting;
   using Microsoft.Extensions.Logging;

   public class BackgroundJobRunnerService : BackgroundService
   {
      private readonly IJobQueue _jobQueue;
      private readonly ILogger<BackgroundJobRunnerService> _logger;

      public BackgroundJobRunnerService(
         IJobQueue jobQueue,
         ILogger<BackgroundJobRunnerService> logger)
      {
         _jobQueue = jobQueue;
         _logger = logger;
      }

      protected override async Task ExecuteAsync(CancellationToken cancellationToken)
      {
         _logger.LogInformation("Background Job Runner Service is starting.");

         while (!cancellationToken.IsCancellationRequested)
         {
            var job = await _jobQueue.DequeueAsync(cancellationToken);

            var _ = Task.Run(async () =>
            {
               try
               {
                  await job.RunAsync(cancellationToken);
               }
               catch (Exception e)
               {
                  _logger.LogError(e, $"Error occurred executing {job.GetType().Name}.");
               }
            }, cancellationToken);
         }

         _logger.LogInformation("Background Job Runner Service is stopping.");
      }
   }
}