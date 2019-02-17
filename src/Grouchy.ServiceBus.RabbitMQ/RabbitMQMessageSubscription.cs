namespace Grouchy.ServiceBus.RabbitMQ
{
   using System;
   using global::RabbitMQ.Client;
   using Grouchy.ServiceBus.Abstractions;

   public class RabbitMQMessageSubscription<TMessage> : DefaultBasicConsumer, IMessageSubscription
      where TMessage : class
   {
      private readonly IModel _channel;
      private readonly IJobQueue _jobQueue;
      private readonly Func<TMessage, IJob> _jobFactory;
      private readonly ISerialisationStrategy _serialisationStrategy;

      private bool _disposed;
      
      public RabbitMQMessageSubscription(
         IModel channel,
         IJobQueue jobQueue,
         Func<TMessage, IJob> jobFactory,
         ISerialisationStrategy serialisationStrategy)
         : base(channel)
      {
         _channel = channel;
         _jobQueue = jobQueue;
         _jobFactory = jobFactory;
         _serialisationStrategy = serialisationStrategy;
      }

      ~RabbitMQMessageSubscription()
      {
         Dispose(false);         
      }

      // TODO: Error handling, retry, ack etc.
      public override void HandleBasicDeliver(
         string consumerTag,
         ulong deliveryTag,
         bool redelivered,
         string exchange,
         string routingKey,
         IBasicProperties properties,
         byte[] body)
      {
         base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

         var message = _serialisationStrategy.Deserialise<TMessage>(body);

         _jobQueue.Enqueue(_jobFactory(message));
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
