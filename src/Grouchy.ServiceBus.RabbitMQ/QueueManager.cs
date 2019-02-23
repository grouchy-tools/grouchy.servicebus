namespace Grouchy.ServiceBus.RabbitMQ
{
   using System.Collections.Concurrent;
   using global::RabbitMQ.Client;

   public class QueueManager
   {
      private readonly ConcurrentDictionary<string, string> _knownQueueNames = new ConcurrentDictionary<string, string>();

      public void EnsureQueueDeclared(IModel channel, string queueName)
      {
         _knownQueueNames.GetOrAdd(queueName, _ => DeclareQueue(channel, queueName));
      }

      private static string DeclareQueue(IModel channel, string queueName)
      {
         // TODO: Exception handling
         channel.QueueDeclare(queueName, true, false, false, null);

         return queueName;
      }
   }
}