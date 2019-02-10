using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using global::RabbitMQ.Client;
using global::RabbitMQ.Client.Events;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.RabbitMQ
{
    public class RabbitMQServiceBus : IServiceBus
    {
        private readonly IConnection _connection;
        private readonly ThreadLocal<IModel> _channel;

        public RabbitMQServiceBus()
        {
            // TODO: Configuration
            var factory = new ConnectionFactory { HostName = "localhost", DispatchConsumersAsync = true };

            _connection = factory.CreateConnection();
            _channel = new ThreadLocal<IModel>(_connection.CreateModel);
        }
        
        public Task Publish<TMessage>(TMessage message)
            where TMessage : class
        {
            var messageType = message.GetType();
            // TODO:
            //var queueName = this.queueNameConvention.GetQueueName(messageType);
            var queueName = messageType.Name.ToLower();

            // TODO: Only declare if not already done so
            _channel.Value.QueueDeclare(queueName, true, false, false, null);
            
            var serialisedMessage = Serialise(message);
            var body = Encoding.UTF8.GetBytes(serialisedMessage);
            
            var properties = _channel.Value.CreateBasicProperties();
            properties.Persistent = true;

            _channel.Value.BasicPublish(string.Empty, queueName, properties, body);

            return Task.CompletedTask;
        }

        public IMessageSubscription Subscribe<TMessage, TMessageHandler>(TMessageHandler messageHandler)
            where TMessage : class
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            var messageType = typeof(TMessage);
            // TODO:
            //var queueName = this.queueNameConvention.GetQueueName(messageType);
            var queueName = messageType.Name.ToLower();

            var channel = _connection.CreateModel();
            var consumerSubscription = new RabbitMQMessageSubscription<TMessage, TMessageHandler>(channel, messageHandler);
            
            // TODO: Only declare if not already done so
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.BasicConsume(queueName, true, consumerSubscription);

            return consumerSubscription;
        }

        private string Serialise<TMessage>(TMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }
        
        // TODO: Tidy up
        public void Dispose()
        {
            _connection?.Close();
        }
    }
}
