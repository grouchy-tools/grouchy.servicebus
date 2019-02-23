namespace Grouchy.ServiceBus.RabbitMQ
{
   using System;
   using System.Threading;
   using System.Threading.Tasks;
   using global::RabbitMQ.Client;

   public class MessageConsumer<TMessage> : AsyncDefaultBasicConsumer
      where TMessage : class
   {
      private readonly IModel _channel;
      private readonly ISerialisationStrategy _serialisationStrategy;
      private readonly IMessageProcessor _messageProcessor;

      private bool _disposed;
      
      public MessageConsumer(
         IModel channel,
         ISerialisationStrategy serialisationStrategy,
         IMessageProcessor messageProcessor)
         : base(channel)
      {
         _channel = channel;
         _serialisationStrategy = serialisationStrategy;
         _messageProcessor = messageProcessor;
      }

      ~MessageConsumer()
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
         await base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

         var message = _serialisationStrategy.Deserialise<TMessage>(body);

         // TODO: Error handling (nack, requeue?, dead letter?)
         // TODO: Logging/metrics
         // TODO: Cancellation token
         await _messageProcessor.ProcessAsync(message, CancellationToken.None);

         // TODO:
         _channel.BasicAck(deliveryTag, false);
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      private void Dispose(bool disposing)
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