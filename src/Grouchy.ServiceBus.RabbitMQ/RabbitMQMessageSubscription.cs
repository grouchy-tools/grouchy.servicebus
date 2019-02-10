using System;
using System.Threading.Tasks;
using global::RabbitMQ.Client;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.RabbitMQ
{
   public class RabbitMQMessageSubscription<TMessage, TMessageHandler> : AsyncDefaultBasicConsumer, IMessageSubscription
      where TMessage : class
      where TMessageHandler : class, IMessageHandler<TMessage>
   {
      private readonly IModel _channel;
      private readonly ISerialisationStrategy _serialisationStrategy;
      private readonly TMessageHandler _messageHandler;

      private bool _disposed = false;
      
      public RabbitMQMessageSubscription(
         IModel channel,
         ISerialisationStrategy serialisationStrategy,
         TMessageHandler messageHandler)
         : base(channel)
      {
         _channel = channel;
         _serialisationStrategy = serialisationStrategy;
         _messageHandler = messageHandler;
      }

      ~RabbitMQMessageSubscription()
      {
         Dispose(false);         
      }

      // TODO: Error handling, retry, ack etc.
      public override async Task HandleBasicDeliver(
         string consumerTag,
         ulong deliveryTag,
         bool redelivered,
         string exchange,
         string routingKey,
         IBasicProperties properties,
         byte[] body)
      {
         await base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body).ConfigureAwait(false);

         var message = _serialisationStrategy.Deserialise<TMessage>(body);

         await _messageHandler.Handle(message);
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (_disposed) return;

         if (disposing)
         {
            _channel.Dispose();
         }

         _disposed = true;
      }
   }
}
