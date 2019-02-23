namespace Grouchy.ServiceBus
{
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;

   public class DefaultMessageProcessor : IMessageProcessor
   {
      private readonly IMessageHandlerFactory _messageHandlerFactory;

      public DefaultMessageProcessor(IMessageHandlerFactory messageHandlerFactory)
      {
         _messageHandlerFactory = messageHandlerFactory;
      }
      
      public async Task ProcessAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
         where TMessage : class
      {
         // TODO: need to create scope before resolving messagehandler - maybe combine handler factory
         
         // TODO: need to support disposable message handlers
         var messageHandler = _messageHandlerFactory.Create<TMessage>();
         
         // TODO: Exception handler should dispose of handler 
         await messageHandler.HandleAsync(message, cancellationToken);
      }
   }
}