namespace Grouchy.ServiceBus.InMemory.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using FakeItEasy;
    using NUnit.Framework;
    using Grouchy.ServiceBus.Abstractions;
    using Grouchy.ServiceBus.AspNetCore;
    using Grouchy.ServiceBus.Testing;

    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase
    {
        private IJobQueue _jobQueue;
        private BackgroundJobRunnerService _jobRunner;
        private ConcurrentMessageQueues _queues;

        [SetUp]
        public void setup_before_each_test()
        {
            _jobQueue = new JobQueue();
            _jobRunner = new BackgroundJobRunnerService(_jobQueue, A.Fake<ILogger<BackgroundJobRunnerService>>());
            _jobRunner.StartAsync(CancellationToken.None);

            _queues = new ConcurrentMessageQueues();
        }

        [TearDown]
        public async Task teardown_after_each_test()
        {
            await _jobRunner.StopAsync(CancellationToken.None);
        }

        protected override Task<IServiceBus> CreateServiceBus()
        {
            return Task.FromResult<IServiceBus>(new InMemoryServiceBus(_jobQueue, _queues));
        }
    }
}