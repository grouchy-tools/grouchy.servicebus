namespace Grouchy.ServiceBus
{
   using Grouchy.ServiceBus.Abstractions;

   public interface IMessageHandlerFactory
   {
      IAsyncMessageHandler<TMessage> Create<TMessage>() where TMessage : class;
   }
}