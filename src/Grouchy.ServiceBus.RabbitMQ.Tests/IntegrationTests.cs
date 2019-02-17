namespace Grouchy.ServiceBus.RabbitMQ.Tests
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
        private RabbitMQConfiguration _configuration;
        private IJobQueue _jobQueue;
        private BackgroundJobRunnerService _jobRunner;
        private IQueueNameStrategy _queueNameStrategy;
        private ISerialisationStrategy _serialisationStrategy;

        [SetUp]
        public void setup_before_each_test()
        {
            _configuration = new RabbitMQConfiguration { Host = "localhost", Port = 5672 };
            _jobQueue = new JobQueue();
            _jobRunner = new BackgroundJobRunnerService(_jobQueue, A.Fake<ILogger<BackgroundJobRunnerService>>());
            _jobRunner.StartAsync(CancellationToken.None);

            _queueNameStrategy = new DefaultQueueNameStrategy();
            _serialisationStrategy = new DefaultSerialisationStrategy();
        }

        [TearDown]
        public async Task teardown_after_each_test()
        {
            await _jobRunner.StopAsync(CancellationToken.None);
        }

        protected override Task<IServiceBus> CreateServiceBus()
        {
            return Task.FromResult<IServiceBus>(new RabbitMQServiceBus(_configuration, _jobQueue, _queueNameStrategy, _serialisationStrategy));
        }
    }
}
