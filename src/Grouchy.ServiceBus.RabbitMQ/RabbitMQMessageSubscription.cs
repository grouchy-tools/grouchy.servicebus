using System;
using System.Text;
using System.Threading.Tasks;
using global::RabbitMQ.Client;
using Newtonsoft.Json;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.RabbitMQ
{
   public class RabbitMQMessageSubscription<TMessage, TMessageHandler> : AsyncDefaultBasicConsumer, IMessageSubscription
      where TMessage : class
      where TMessageHandler : class, IMessageHandler<TMessage>
   {
      private readonly IModel _channel;
      private readonly TMessageHandler _messageHandler;

      private bool _disposed = false;
      
      public RabbitMQMessageSubscription(
         IModel channel,
         TMessageHandler messageHandler)
         : base(channel)
      {
         _channel = channel;
         _messageHandler = messageHandler;
      }

      ~RabbitMQMessageSubscription()
      {
         Dispose(false);         
      }

      // TODO: Error handling, retry etc.
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

         var serialisedMessage = Encoding.UTF8.GetString(body);
         var message = Deserialise(serialisedMessage);

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
      
      private TMessage Deserialise(string serialisedMessage)
      {
         return JsonConvert.DeserializeObject<TMessage>(serialisedMessage);
      }
   }
}
