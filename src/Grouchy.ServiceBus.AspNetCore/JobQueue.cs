namespace Grouchy.ServiceBus.AspNetCore
{
   using System;
   using System.Collections.Concurrent;
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;

   public class JobQueue : IJobQueue
   {
      private readonly ConcurrentQueue<IJob> _queue = new ConcurrentQueue<IJob>();
      private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

      public void Enqueue(IJob job)
      {
         if (job == null) throw new ArgumentNullException(nameof(job));
         
         _queue.Enqueue(job);
         _signal.Release();
      }

      public async Task<IJob> DequeueAsync(CancellationToken cancellationToken)
      {
         while (!cancellationToken.IsCancellationRequested)
         {
            await _signal.WaitAsync(cancellationToken);

            if (_queue.TryDequeue(out var job))
            {
               return job;
            }
         }

         throw new TaskCanceledException();
      }
   }
}