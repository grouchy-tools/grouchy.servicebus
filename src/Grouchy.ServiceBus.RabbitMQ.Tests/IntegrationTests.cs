using System.Threading.Tasks;
using NUnit.Framework;
using Grouchy.ServiceBus.Abstractions;
using Grouchy.ServiceBus.Testing;

namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase
    {
        private IQueueNameStrategy _queueNameStrategy;
        private ISerialisationStrategy _serialisationStrategy;

        [SetUp]
        public void setup_before_each_test()
        {
            _queueNameStrategy = new DefaultQueueNameStrategy();
            _serialisationStrategy = new DefaultSerialisationStrategy();
        }

        protected override Task<IServiceBus> CreateServiceBus()
        {
            return Task.FromResult<IServiceBus>(new RabbitMQServiceBus(_queueNameStrategy, _serialisationStrategy));
        }
    }
}
