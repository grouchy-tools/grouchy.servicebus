using System;
using System.Collections.Concurrent;

namespace Grouchy.ServiceBus.InMemory
{
   public class ConcurrentMessageQueues
   {
      private readonly ConcurrentDictionary<Type, object> _queues;

      public ConcurrentMessageQueues()
      {
         _queues = new ConcurrentDictionary<Type, object>();
      }

      public BlockingCollection<TMessage> GetQueue<TMessage>(bool create = false)
         where TMessage : class
      {
         object queue;
         
         if (create)
         {
            queue = _queues.GetOrAdd(typeof(TMessage), _ => new BlockingCollection<TMessage>());
         }
         else
         {
            _queues.TryGetValue(typeof(TMessage), out queue);
         }

         return (BlockingCollection<TMessage>)queue;
      }
   }
}