namespace Grouchy.ServiceBus.RabbitMQ
{
   using System;
   using System.Collections.Concurrent;
   using global::RabbitMQ.Client;
   using Grouchy.ServiceBus.Abstractions;

   public class MessageSubscription<TMessage> : IMessageSubscription
      where TMessage : class
   {
      private readonly QueueManager _queueManager;
      private readonly ISerialisationStrategy _serialisationStrategy;
      private readonly IMessageProcessor _messageProcessor;
      private readonly ConcurrentBag<MessageConsumer<TMessage>> _consumers = new ConcurrentBag<MessageConsumer<TMessage>>();

      private bool _disposed;

      public MessageSubscription(
         QueueManager queueManager,
         ISerialisationStrategy serialisationStrategy,
         IMessageProcessor messageProcessor)
      {
         _queueManager = queueManager;
         _serialisationStrategy = serialisationStrategy;
         _messageProcessor = messageProcessor;
      }
      
      ~MessageSubscription()
      {
         Dispose(false);         
      }

      public void AddConsumer(IModel channel, string queueName)
      {
         _queueManager.EnsureQueueDeclared(channel, queueName);

         var consumer = new MessageConsumer<TMessage>(channel, _serialisationStrategy, _messageProcessor);
         _consumers.Add(consumer);

         // Set prefetch to avoid all messages being dumped onto a single consumer
         channel.BasicQos(0, 1, false);
         channel.BasicConsume(queueName, false, consumer);
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
            foreach (var consumer in _consumers)
            {
               consumer.Dispose();
            }
         }

         _disposed = true;
      }
   }
}
