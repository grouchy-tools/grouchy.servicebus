using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using NUnit.Framework;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.Testing
{
   public abstract class IntegrationTestsBase
   {
      protected abstract Task<IServiceBus> CreateServiceBus();

      // TODO: Receive on different bus instances
      
      [Test]
      public async Task publish_before_subscribe()
      {
         var id = Guid.NewGuid().ToString().Substring(8);
         
         var sequences = new ConcurrentBag<TestMessage>();
         var messageHandler = new TestMessageHandler(sequences);

         using (var serviceBus = await CreateServiceBus())
         {
            await serviceBus.Publish(new TestMessage {Id = id});
            
            serviceBus.Subscribe<TestMessage, TestMessageHandler>(messageHandler);
            
            await WaitForSubscribers(sequences, 1);
         }
         
         Assert.That(sequences.Count, Is.EqualTo(1));
         Assert.That(sequences.Single().Id, Is.EqualTo(id));
      }

      [Test]
      public async Task subscribe_before_publish()
      {
         var id = Guid.NewGuid().ToString().Substring(8);
         
         var sequences = new ConcurrentBag<TestMessage>();
         var messageHandler = new TestMessageHandler(sequences);

         using (var serviceBus = await CreateServiceBus())
         {
            serviceBus.Subscribe<TestMessage, TestMessageHandler>(messageHandler);
            
            await serviceBus.Publish(new TestMessage {Id = id});
            
            await WaitForSubscribers(sequences, 1);
         }
         
         Assert.That(sequences.Count, Is.EqualTo(1));
         Assert.That(sequences.Single().Id, Is.EqualTo(id));
      }

      [Test]
      public async Task dispose_subscription_before_publish()
      {
         var id = Guid.NewGuid().ToString().Substring(8);
         
         var sequences = new ConcurrentBag<TestMessage>();
         var messageHandler = new TestMessageHandler(sequences);

         using (var serviceBus = await CreateServiceBus())
         {
            var subscription = serviceBus.Subscribe<TestMessage, TestMessageHandler>(messageHandler);
            subscription.Dispose();
            
            await serviceBus.Publish(new TestMessage {Id = id});

            await Task.Delay(100);
            
            serviceBus.Subscribe<TestMessage, NullMessageHandler>(new NullMessageHandler());

            await Task.Delay(100);
         }
         
         Assert.That(sequences.Count, Is.EqualTo(0));
      }

      [Test]
      public async Task multiple_messages_are_all_sent_and_received_in_same_bus()
      {
         const int messages = 100;
         
         var sequences = new ConcurrentBag<TestMessage>();
         var messageHandler = new TestMessageHandler(sequences);

         using (var serviceBus = await CreateServiceBus())
         {
            serviceBus.Subscribe<TestMessage, TestMessageHandler>(messageHandler);

            var publishTasks = Enumerable.Range(0, messages).Select(i => serviceBus.Publish(new TestMessage {Sequence = i}));
            await Task.WhenAll(publishTasks);

            await WaitForSubscribers(sequences, messages);
         }
         
         Assert.That(sequences.Count, Is.EqualTo(messages));
         Assert.That(sequences.Select(c => c.Sequence).Distinct().Count, Is.EqualTo(messages));
      }

      [Test]
      public async Task multiple_messages_are_sent_from_and_received_by_different_bus()
      {
         const int messages = 100;
         
         var sequences = new ConcurrentBag<TestMessage>();
         var messageHandler = new TestMessageHandler(sequences);
         
         using (var sendServiceBus = await CreateServiceBus())
         using (var receiveServiceBus = await CreateServiceBus())
         {
            receiveServiceBus.Subscribe<TestMessage, TestMessageHandler>(messageHandler);

            var publishTasks = Enumerable.Range(0, messages).Select(i => sendServiceBus.Publish(new TestMessage {Sequence = i}));
            await Task.WhenAll(publishTasks);

            await WaitForSubscribers(sequences, messages);
         }
         
         Assert.That(sequences.Count, Is.EqualTo(messages));
         Assert.That(sequences.Select(c => c.Sequence).Distinct().Count, Is.EqualTo(messages));
      }
      
      private static async Task WaitForSubscribers(IReadOnlyCollection<TestMessage> messages, int expectedCount)
      {
         var j = 0;
         while (j < 10 && messages.Count < expectedCount)
         {
            await Task.Delay(200);
            j++;
         }
      }

      public class TestMessage
      {
         public int Sequence { get; set; }
         
         public string Id { get; set; }
      }

      public class TestMessageHandler : IMessageHandler<TestMessage>
      {
         private readonly ConcurrentBag<TestMessage> _messages;

         public TestMessageHandler(ConcurrentBag<TestMessage> messages)
         {
            _messages = messages;
         }

         public Task Handle(TestMessage message)
         {
            _messages.Add(message);

            return Task.CompletedTask;
         }
      }

      public class NullMessageHandler : IMessageHandler<TestMessage>
      {
         public Task Handle(TestMessage message)
         {
            return Task.CompletedTask;
         }
      }
   }
}