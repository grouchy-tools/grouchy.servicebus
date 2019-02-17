namespace Grouchy.ServiceBus.InMemory
{
   using System;
   using System.Collections.Concurrent;

   public class ConcurrentMessageQueues
   {
      private readonly ConcurrentDictionary<Type, object> _queues;

      public ConcurrentMessageQueues()
      {
         _queues = new ConcurrentDictionary<Type, object>();
      }

      public BlockingCollection<TMessage> GetQueue<TMessage>()
         where TMessage : class
      {
         var queue = _queues.GetOrAdd(typeof(TMessage), _ => new BlockingCollection<TMessage>());

         return (BlockingCollection<TMessage>)queue;
      }
   }
}