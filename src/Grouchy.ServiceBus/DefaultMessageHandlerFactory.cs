namespace Grouchy.ServiceBus
{
   using System;
   using Grouchy.ServiceBus.Abstractions;

   public class DefaultMessageHandlerFactory : IMessageHandlerFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public DefaultMessageHandlerFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }
      
      public IAsyncMessageHandler<TMessage> Create<TMessage>()
         where TMessage : class
      {
         var messageHandler = (IAsyncMessageHandler<TMessage>)_serviceProvider.GetService(typeof(IAsyncMessageHandler<TMessage>));
         return messageHandler;
      }
   }
}