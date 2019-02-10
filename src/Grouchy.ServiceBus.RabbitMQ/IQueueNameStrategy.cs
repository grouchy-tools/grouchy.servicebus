using System;

namespace Grouchy.ServiceBus.RabbitMQ
{
   public interface IQueueNameStrategy
   {
      string GetQueueName(Type messageType);
   }
}