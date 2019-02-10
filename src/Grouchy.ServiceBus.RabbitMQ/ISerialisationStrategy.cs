namespace Grouchy.ServiceBus.RabbitMQ
{
   public interface ISerialisationStrategy
   {
      byte[] Serialise<TMessage>(TMessage message);
        
      TMessage Deserialise<TMessage>(byte[] body);
   }
}