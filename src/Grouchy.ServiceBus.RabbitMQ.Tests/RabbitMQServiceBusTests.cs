using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Grouchy.ServiceBus.Abstractions;
using NUnit.Framework;
using RabbitMQ.Client;

namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
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
            
            _testSubject = new RabbitMQServiceBus(_connectionFactory, _queueNameStrategy, _serialisationStrategy);
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
            _testSubject.Subscribe<TestMessage, TestMessageHandler>(new TestMessageHandler());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public void does_not_create_queue_on_second_subscribe()
        {
            _testSubject.Subscribe<TestMessage, TestMessageHandler>(new TestMessageHandler());
            _testSubject.Subscribe<TestMessage, TestMessageHandler>(new TestMessageHandler());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task does_not_create_queue_on_subscribe_after_publish()
        {
            await _testSubject.Publish(new TestMessage());
            _testSubject.Subscribe<TestMessage, TestMessageHandler>(new TestMessageHandler());

            A.CallTo(() => _channel.QueueDeclare(A<string>._, A<bool>._, A<bool>._, A<bool>._, A<IDictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task does_not_create_queue_on_publish_after_subscribe()
        {
            _testSubject.Subscribe<TestMessage, TestMessageHandler>(new TestMessageHandler());
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
