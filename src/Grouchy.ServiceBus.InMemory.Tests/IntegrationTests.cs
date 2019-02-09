using System.Threading.Tasks;
using NUnit.Framework;
using Grouchy.ServiceBus.Abstractions;
using Grouchy.ServiceBus.Testing;

namespace Grouchy.ServiceBus.InMemory.Tests
{
    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase
    {
        private InMemoryServiceBusQueues _queues;

        [SetUp]
        public void setup_before_each_test()
        {
            _queues = new InMemoryServiceBusQueues();
        }
        
        protected override Task<IServiceBus> CreateServiceBus()
        {
            return Task.FromResult<IServiceBus>(new InMemoryServiceBus(_queues));
        }
    }
}