using System.Threading.Tasks;
using NUnit.Framework;
using Grouchy.ServiceBus.Abstractions;
using Grouchy.ServiceBus.Testing;

namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase
    {
        protected override Task<IServiceBus> CreateServiceBus()
        {
            return Task.FromResult<IServiceBus>(new RabbitMQServiceBus());
        }
    }
}