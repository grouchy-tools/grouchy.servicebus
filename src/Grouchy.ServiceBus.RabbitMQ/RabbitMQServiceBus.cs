namespace Grouchy.ServiceBus.RabbitMQ
{
   using System;
   using System.Threading;
   using System.Threading.Tasks;
   using global::RabbitMQ.Client;
   using Grouchy.ServiceBus.Abstractions;

   // TODO: should two handlers for the same message create different queues. Thus when a message is received from
   // a queue there will be only 1 handler
   
   public class RabbitMQServiceBus : IServiceBus
   {
      private readonly IQueueNameStrategy _queueNameStrategy;
      private readonly ISerialisationStrategy _serialisationStrategy;
      private readonly IMessageProcessor _messageProcessor;
      private readonly Lazy<IConnection> _publishConnection;
      private readonly Lazy<IConnection> _consumeConnection;
      private readonly ResourcePool<IModel> _publishChannels;
      private readonly QueueManager _queueManager;
      
      public RabbitMQServiceBus(
         RabbitMQConfiguration configuration,
         IQueueNameStrategy queueNameStrategy,
         ISerialisationStrategy serialisationStrategy,
         IMessageProcessor messageProcessor)
         : this(new ConnectionFactory {HostName = configuration.Host, Port = configuration.Port, DispatchConsumersAsync = true}, queueNameStrategy, serialisationStrategy, messageProcessor)
      {
      }

      public RabbitMQServiceBus(
         IConnectionFactory connectionFactory,
         IQueueNameStrategy queueNameStrategy,
         ISerialisationStrategy serialisationStrategy,
         IMessageProcessor messageProcessor)
      {
         _queueNameStrategy = queueNameStrategy;
         _serialisationStrategy = serialisationStrategy;
         _messageProcessor = messageProcessor;
         _publishConnection = new Lazy<IConnection>(connectionFactory.CreateConnection, LazyThreadSafetyMode.PublicationOnly);
         _consumeConnection = new Lazy<IConnection>(connectionFactory.CreateConnection, LazyThreadSafetyMode.PublicationOnly);
         // TODO: Is 10 enough? Add to config
         _publishChannels = new ResourcePool<IModel>(10,() => _publishConnection.Value.CreateModel());
         _queueManager = new QueueManager();
      }

      public async Task Publish<TMessage>(TMessage message)
         where TMessage : class
      {
         var queueName = _queueNameStrategy.GetQueueName(message.GetType());

         using (var channel = await _publishChannels.AllocateAsync())
         {
            // TODO: Should be sending to exchange rather than to queue
            _queueManager.EnsureQueueDeclared(channel.Value, queueName);

            var properties = channel.Value.CreateBasicProperties();
            properties.Persistent = true;

            var body = _serialisationStrategy.Serialise(message);

            channel.Value.BasicPublish(string.Empty, queueName, properties, body);
         }
      }

      // TODO: SubscriptionOptions - no of concurrent consumers
      public IMessageSubscription Subscribe<TMessage>()
         where TMessage : class
      {
         var queueName = _queueNameStrategy.GetQueueName(typeof(TMessage));

         var subscription = new MessageSubscription<TMessage>(_queueManager, _serialisationStrategy, _messageProcessor);

         for (var i = 0; i < 10; i++)
         {
            subscription.AddConsumer(_consumeConnection.Value.CreateModel(), queueName);
         }
         
         return subscription;
      }

      // TODO: Tidy up
      public void Dispose()
      {
         if (_publishConnection.IsValueCreated)
         {
            _publishConnection.Value.Dispose();
         }
         if (_consumeConnection.IsValueCreated)
         {
            _consumeConnection.Value.Dispose();
         }
      }  
   }
}