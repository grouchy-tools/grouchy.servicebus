namespace Grouchy.ServiceBus.RabbitMQ
{
   using System.Collections.Concurrent;
   using System.Threading;
   using System.Threading.Tasks;
   using global::RabbitMQ.Client;
   using Grouchy.ServiceBus.Abstractions;

   public class RabbitMQServiceBus : IServiceBus
   {
      private readonly IJobQueue _jobQueue;
      private readonly IQueueNameStrategy _queueNameStrategy;
      private readonly ISerialisationStrategy _serialisationStrategy;
      private readonly IConnection _connection;
      private readonly ThreadLocal<IModel> _channel;
      private readonly ConcurrentDictionary<string, string> _knownQueueNames;

      public RabbitMQServiceBus(
         RabbitMQConfiguration configuration,
         IJobQueue jobQueue,
         IQueueNameStrategy queueNameStrategy,
         ISerialisationStrategy serialisationStrategy)
         : this(new ConnectionFactory {HostName = configuration.Host, Port = configuration.Port}, jobQueue, queueNameStrategy, serialisationStrategy)
      {
      }

      public RabbitMQServiceBus(
         IConnectionFactory connectionFactory,
         IJobQueue jobQueue,
         IQueueNameStrategy queueNameStrategy,
         ISerialisationStrategy serialisationStrategy)
      {
         _jobQueue = jobQueue;
         _queueNameStrategy = queueNameStrategy;
         _serialisationStrategy = serialisationStrategy;
         _connection = connectionFactory.CreateConnection();
         _channel = new ThreadLocal<IModel>(_connection.CreateModel);
         _knownQueueNames = new ConcurrentDictionary<string, string>();
      }

      public Task Publish<TMessage>(TMessage message)
         where TMessage : class
      {
         var queueName = _queueNameStrategy.GetQueueName(message.GetType());
         EnsureQueueDeclared(_channel.Value, queueName);

         var properties = _channel.Value.CreateBasicProperties();
         properties.Persistent = true;

         var body = _serialisationStrategy.Serialise(message);

         _channel.Value.BasicPublish(string.Empty, queueName, properties, body);

         return Task.CompletedTask;
      }

      public IMessageSubscription Subscribe<TMessage>(IMessageHandler<TMessage> messageHandler)
         where TMessage : class
      {
         var queueName = _queueNameStrategy.GetQueueName(typeof(TMessage));

         var channel = _connection.CreateModel();         
         EnsureQueueDeclared(channel, queueName);

         IJob JobFactory(TMessage message) => new MessageHandlerJob<TMessage>(message, messageHandler);
         
         var consumerSubscription = new RabbitMQMessageSubscription<TMessage>(channel, _jobQueue, JobFactory, _serialisationStrategy);
         channel.BasicConsume(queueName, true, consumerSubscription);

         return consumerSubscription;
      }

      // TODO: Tidy up
      public void Dispose()
      {
         _connection?.Close();
      }
      
      private void EnsureQueueDeclared(IModel channel, string queueName)
      {
         _knownQueueNames.GetOrAdd(queueName, _ => DeclareQueue(channel, queueName));
      }

      private static string DeclareQueue(IModel channel, string queueName)
      {
         // TODO: Exception handling
         channel.QueueDeclare(queueName, true, false, false, null);

         return queueName;
      }

      private class MessageHandlerJob<TMessage> : IJob
         where TMessage : class
      {
         private readonly TMessage _message;
         private readonly IMessageHandler<TMessage> _messageHandler;

         public MessageHandlerJob(TMessage message, IMessageHandler<TMessage> messageHandler)
         {
            _messageHandler = messageHandler;
            _message = message;
         }
         
         public async Task RunAsync(CancellationToken cancellationToken)
         {
            // TODO: Add cancellationToken to Handle method
            // TODO: Error handling
            // TODO: ack/nack
            await _messageHandler.Handle(_message);
         }
      }
   }
}