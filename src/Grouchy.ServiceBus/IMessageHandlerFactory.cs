namespace Grouchy.ServiceBus
{
   using Grouchy.ServiceBus.Abstractions;

   public interface IMessageHandlerFactory
   {
      IMessageHandler<TMessage> Create<TMessage>() where TMessage : class;
   }
}