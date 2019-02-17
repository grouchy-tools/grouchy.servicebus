using System.Threading.Tasks;
using NUnit.Framework;
using Grouchy.ServiceBus.Abstractions;
using Grouchy.ServiceBus.Testing;

namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase
    {
        private RabbitMQConfiguration _configuration;
        private IQueueNameStrategy _queueNameStrategy;
        private ISerialisationStrategy _serialisationStrategy;

        [SetUp]
        public void setup_before_each_test()
        {
            _configuration = new RabbitMQConfiguration { Host = "localhost", Port = 5672 };
            _queueNameStrategy = new DefaultQueueNameStrategy();
            _serialisationStrategy = new DefaultSerialisationStrategy();
        }

        protected override Task<IServiceBus> CreateServiceBus()
        {
            return Task.FromResult<IServiceBus>(new RabbitMQServiceBus(_configuration, _queueNameStrategy, _serialisationStrategy));
        }
    }
}
