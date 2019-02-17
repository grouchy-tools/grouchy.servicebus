namespace Grouchy.ServiceBus.RabbitMQ
{
   using System;

   public interface IQueueNameStrategy
   {
      string GetQueueName(Type messageType);
   }
}