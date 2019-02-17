namespace Grouchy.ServiceBus.RabbitMQ
{
   using System;
   using System.Collections.Concurrent;
   using System.Linq;
   using Grouchy.ServiceBus.Abstractions;

   public class DefaultQueueNameStrategy : IQueueNameStrategy
   {
      private readonly ConcurrentDictionary<Type, string> _queueNames;

      public DefaultQueueNameStrategy()
      {
         _queueNames = new ConcurrentDictionary<Type, string>();
      }

      public string GetQueueName(Type messageType)
      {
         var queueName = _queueNames.GetOrAdd(messageType, GetQueueNameFromMessageType);
         return queueName;            
      }

      private static string GetQueueNameFromMessageType(Type messageType)
      {
         if (messageType.GetCustomAttributes(typeof(QueueNameAttribute), true).FirstOrDefault() is QueueNameAttribute attribute)
         {
            return attribute.Name;
         }

         return messageType.FullName;
      }
   }
}