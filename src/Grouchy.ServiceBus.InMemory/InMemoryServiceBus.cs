using System.Threading;
using System.Threading.Tasks;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.InMemory
{
   public class InMemoryServiceBus : IServiceBus
   {
      private readonly ConcurrentMessageQueues _queues;

      public InMemoryServiceBus(ConcurrentMessageQueues queues)
      {
         _queues = queues;
      }

      public Task Publish<TMessage>(TMessage message)
         where TMessage : class
      {
         var queue = _queues.GetQueue<TMessage>();

         queue.Add(message);

         return Task.CompletedTask;
      }

      public IMessageSubscription Subscribe<TMessage, TMessageHandler>(TMessageHandler messageHandler)
         where TMessage : class
         where TMessageHandler : class, IMessageHandler<TMessage>
      {
         var queue = _queues.GetQueue<TMessage>();

         var cancellationTokenSource = new CancellationTokenSource();
         var cancellationToken = cancellationTokenSource.Token;

         Task.Run(async () =>
         {            
            while (!cancellationToken.IsCancellationRequested)
            {
               var message = queue.Take(cancellationToken);

               await messageHandler.Handle(message);
            }
         }, cancellationToken);

         return new InMemoryMessageSubscription(cancellationTokenSource);
      }

      // TODO: Tidy up
      public void Dispose()
      {
      }
   }
}