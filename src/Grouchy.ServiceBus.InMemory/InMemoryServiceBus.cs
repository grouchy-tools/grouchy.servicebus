using System.Threading;
using System.Threading.Tasks;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.InMemory
{
   public class InMemoryServiceBus : IServiceBus
   {
      private readonly InMemoryServiceBusQueues _queues;

      public InMemoryServiceBus(InMemoryServiceBusQueues queues)
      {
         _queues = queues;
      }

      public Task Publish<TMessage>(TMessage message)
         where TMessage : class
      {
         var queue = _queues.GetQueue<TMessage>();

         queue?.Add(message);

         return Task.CompletedTask;
      }

      public IMessageSubscription Subscribe<TMessage, TMessageHandler>(TMessageHandler messageHandler)
         where TMessage : class
         where TMessageHandler : class, IMessageHandler<TMessage>
      {
         var queue = _queues.GetQueue<TMessage>(true);

         Task.Run(async () =>
         {
            var cancellationToken = new CancellationToken();
            
            while (!cancellationToken.IsCancellationRequested)
            {
               var message = (TMessage)queue.Take(cancellationToken);

               await messageHandler.Handle(message);
            }
         });

         // TODO: Add disposable subscription
         return null;
      }

      // TODO: Tidy up
      public void Dispose()
      {
      }
   }
}