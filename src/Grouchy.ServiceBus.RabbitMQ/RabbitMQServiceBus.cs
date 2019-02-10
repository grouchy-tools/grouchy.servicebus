using System.Threading;
using System.Threading.Tasks;
using global::RabbitMQ.Client;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.RabbitMQ
{
   public class RabbitMQServiceBus : IServiceBus
   {
      private readonly IQueueNameStrategy _queueNameStrategy;
      private readonly ISerialisationStrategy _serialisationStrategy;
      private readonly IConnection _connection;
      private readonly ThreadLocal<IModel> _channel;

      public RabbitMQServiceBus(
         IQueueNameStrategy queueNameStrategy,
         ISerialisationStrategy serialisationStrategy)
      {
         _queueNameStrategy = queueNameStrategy;
         _serialisationStrategy = serialisationStrategy;

         // TODO: Configuration
         var factory = new ConnectionFactory {HostName = "localhost", DispatchConsumersAsync = true};

         _connection = factory.CreateConnection();
         _channel = new ThreadLocal<IModel>(_connection.CreateModel);
      }

      public Task Publish<TMessage>(TMessage message)
         where TMessage : class
      {
         var queueName = _queueNameStrategy.GetQueueName(message.GetType());

         // TODO: Only declare if not already done so
         _channel.Value.QueueDeclare(queueName, true, false, false, null);

         var properties = _channel.Value.CreateBasicProperties();
         properties.Persistent = true;

         var body = _serialisationStrategy.Serialise(message);

         _channel.Value.BasicPublish(string.Empty, queueName, properties, body);

         return Task.CompletedTask;
      }

      public IMessageSubscription Subscribe<TMessage, TMessageHandler>(TMessageHandler messageHandler)
         where TMessage : class
         where TMessageHandler : class, IMessageHandler<TMessage>
      {
         var queueName = _queueNameStrategy.GetQueueName(typeof(TMessage));

         var channel = _connection.CreateModel();
         var consumerSubscription = new RabbitMQMessageSubscription<TMessage, TMessageHandler>(channel, _serialisationStrategy, messageHandler);

         // TODO: Only declare if not already done so
         channel.QueueDeclare(queueName, true, false, false, null);
         channel.BasicConsume(queueName, true, consumerSubscription);

         return consumerSubscription;
      }

      // TODO: Tidy up
      public void Dispose()
      {
         _connection?.Close();
      }
   }
}