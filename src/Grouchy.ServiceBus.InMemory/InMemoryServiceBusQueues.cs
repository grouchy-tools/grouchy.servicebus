using System;
using System.Collections.Concurrent;

namespace Grouchy.ServiceBus.InMemory
{
   public class InMemoryServiceBusQueues
   {
      private readonly ConcurrentDictionary<Type, BlockingCollection<object>> _queues;

      public InMemoryServiceBusQueues()
      {
         _queues = new ConcurrentDictionary<Type, BlockingCollection<object>>();

      }

      public BlockingCollection<object> GetQueue<TMessage>(bool create = false)
         where TMessage : class
      {
         if (create)
         {
            return _queues.GetOrAdd(
               typeof(TMessage),
               _ => new BlockingCollection<object>());
         }

         _queues.TryGetValue(typeof(TMessage), out var queue);

         return queue;
      }
   }
}