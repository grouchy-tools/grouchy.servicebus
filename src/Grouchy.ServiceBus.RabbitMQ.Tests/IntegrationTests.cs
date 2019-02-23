namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using global::RabbitMQ.Client;
    using Grouchy.ServiceBus.Abstractions;
    using Grouchy.ServiceBus.Testing;

    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase
    {
        private RabbitMQConfiguration _configuration;

        [OneTimeSetUp]
        public async Task setup_before_all_tests()
        {
            var connectionFactory = new ConnectionFactory {HostName = "localhost", Port = 5672};

            while (true)
            {
                try
                {
                    using (var connection = connectionFactory.CreateConnection())
                    {
                        if (connection.IsOpen) break;
                    }
                    
                    await Task.Delay(1000);
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
        }
        
        [SetUp]
        public void setup_before_each_test()
        {
            _configuration = new RabbitMQConfiguration { Host = "localhost", Port = 5672 };
        }

        protected override Task<IServiceBus> CreateServiceBus(IServiceProvider sp)
        {
            return Task.FromResult<IServiceBus>(new RabbitMQServiceBus(_configuration, new DefaultQueueNameStrategy(), new DefaultSerialisationStrategy(), new DefaultMessageProcessor(new DefaultMessageHandlerFactory(sp))));
        }
    }
}
