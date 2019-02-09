using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.InMemory
{
   public class InMemoryServiceBus : IServiceBus
   {
      private readonly ConcurrentDictionary<Type, BlockingCollection<object>> _queues;

      public InMemoryServiceBus()
      {
         _queues = new ConcurrentDictionary<Type, BlockingCollection<object>>();
      }

      public Task Publish<TMessage>(TMessage message)
         where TMessage : class
      {
         _queues.TryGetValue(message.GetType(), out var queue);

         queue?.Add(message);

         return Task.CompletedTask;
      }

      public IMessageSubscription Subscribe<TMessage, TMessageHandler>(TMessageHandler messageHandler)
         where TMessage : class
         where TMessageHandler : class, IMessageHandler<TMessage>
      {
         var queue = _queues.GetOrAdd(
            typeof(TMessage),
            _ => new BlockingCollection<object>());

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