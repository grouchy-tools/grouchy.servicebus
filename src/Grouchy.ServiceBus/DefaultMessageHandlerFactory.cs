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
      
      public IMessageHandler<TMessage> Create<TMessage>()
         where TMessage : class
      {
         var messageHandler = (IMessageHandler<TMessage>)_serviceProvider.GetService(typeof(IMessageHandler<TMessage>));
         return messageHandler;
      }
   }
}