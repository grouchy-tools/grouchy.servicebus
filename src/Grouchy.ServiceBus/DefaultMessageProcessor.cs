namespace Grouchy.ServiceBus
{
   using System;
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;
   using Microsoft.Extensions.DependencyInjection;

   public class DefaultMessageProcessor : IMessageProcessor
   {
      private readonly IServiceProvider _serviceProvider;

      public DefaultMessageProcessor(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }
      
      public async Task ProcessAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
         where TMessage : class
      {
         // TODO: request id (rabbit correlation id??), correlation id (not rabbit correlation id??), session id, device id
         
         // TODO: write metrics before, after and on exception
         
         // Create new scope for any resolutions of the handler or inside the handler
         using (var scope = _serviceProvider.CreateScope())
         {
            var messageHandler = CreateHandlerFor<TMessage>(scope.ServiceProvider);
         
            await messageHandler.HandleAsync(message, cancellationToken);
         }
      }
      
      private static IMessageHandler<TMessage> CreateHandlerFor<TMessage>(IServiceProvider scope)
         where TMessage : class
      {
         var messageHandler = (IMessageHandler<TMessage>)scope.GetService(typeof(IMessageHandler<TMessage>));
         return messageHandler;
      }
   }
}