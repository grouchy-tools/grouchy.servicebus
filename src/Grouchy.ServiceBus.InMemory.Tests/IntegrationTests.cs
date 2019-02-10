using System.Threading.Tasks;
using NUnit.Framework;
using Grouchy.ServiceBus.Abstractions;
using Grouchy.ServiceBus.Testing;

namespace Grouchy.ServiceBus.InMemory.Tests
{
    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase
    {
        private ConcurrentMessageQueues _queues;

        [SetUp]
        public void setup_before_each_test()
        {
            _queues = new ConcurrentMessageQueues();
        }
        
        protected override Task<IServiceBus> CreateServiceBus()
        {
            return Task.FromResult<IServiceBus>(new InMemoryServiceBus(_queues));
        }
    }
}