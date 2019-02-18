namespace Grouchy.ServiceBus.RabbitMQ
{
   public class RabbitMQConfiguration
   {
      public string Host { get; set; } = "localhost";

      public int Port { get; set; } = 5672;
   }
}