namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using NUnit.Framework;
    using global::RabbitMQ.Client;
    using Grouchy.ServiceBus.Abstractions;

    public class RabbitMQServiceBusTests
    {
        private IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private IQueueNameStrategy _queueNameStrategy;
        private ISerialisationStrategy _serialisationStrategy;
        private RabbitMQServiceBus _testSubject;

        [SetUp]
        public void setup_before_each_test()
        {
            _connectionFactory = A.Fake<IConnectionFactory>();
            _connection = A.Fake<IConnection>();
            _channel = A.Fake<IModel>();
            _queueNameStrategy = new DefaultQueueNameStrategy();
            _serialisationStrategy = new DefaultSerialisationStrategy();

            A.CallTo(() => _connectionFactory.CreateConnection()).Returns(_connection);
            A.CallTo(() => _connection.CreateModel()).Returns(_channel);
            
            _testSubject = new RabbitMQServiceBus(_connectionFactory, A.Fake<IJobQueue>(), _queueNameStrategy, _serialisationStrategy);
        }

        [Test]
        public async Task declares_queue_on_first_publish()
        {
            await _testSubject.Publish(new TestMessage());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task does_not_declare_queue_on_second_publish()
        {
            await _testSubject.Publish(new TestMessage());
            await _testSubject.Publish(new TestMessage());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void creates_queue_on_first_subscribe()
        {
            _testSubject.Subscribe<TestMessage>(new TestMessageHandler());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public void does_not_create_queue_on_second_subscribe()
        {
            _testSubject.Subscribe<TestMessage>(new TestMessageHandler());
            _testSubject.Subscribe<TestMessage>(new TestMessageHandler());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task does_not_create_queue_on_subscribe_after_publish()
        {
            await _testSubject.Publish(new TestMessage());
            _testSubject.Subscribe<TestMessage>(new TestMessageHandler());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task does_not_create_queue_on_publish_after_subscribe()
        {
            _testSubject.Subscribe<TestMessage>(new TestMessageHandler());
            await _testSubject.Publish(new TestMessage());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }
        
        public class TestMessage
        {
        }

        public class TestMessageHandler : IMessageHandler<TestMessage>
        {
            public Task Handle(TestMessage message)
            {
                return Task.CompletedTask;
            }
        }
    }
}
