namespace Grouchy.ServiceBus.Testing
{
   using System;
   using System.Collections.Generic;
   using System.Collections.Concurrent;
   using System.Diagnostics;
   using System.Threading.Tasks;
   using System.Linq;
   using NUnit.Framework;
   using Grouchy.ServiceBus.Abstractions;
   using Grouchy.ServiceBus.Testing.Handlers;
   using Grouchy.ServiceBus.Testing.Messages;

   public abstract class IntegrationTestsBase
   {
      protected abstract Task<IServiceBus> CreateServiceBus();

      // TODO: Receive on different bus instances
      // TODO: Send vs Publish
      // TODO: Subscribe overload without argument
      // TODO: Handling unroutable messages https://www.rabbitmq.com/dotnet-api-guide.html
      // TODO: manual ack/nack
      
      [Test]
      public async Task publish_before_subscribe()
      {
         var id = Guid.NewGuid().ToString().Substring(8);
         
         var sequences = new ConcurrentBag<TestMessage>();
         var messageHandler = new TestMessageHandler(sequences);

         using (var serviceBus = await CreateServiceBus())
         {
            await serviceBus.Publish(new TestMessage {Id = id});
            
            serviceBus.Subscribe<TestMessage>(messageHandler);
            
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
            serviceBus.Subscribe<TestMessage>(messageHandler);
            
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
            var subscription = serviceBus.Subscribe<TestMessage>(messageHandler);
            subscription.Dispose();
            
            await serviceBus.Publish(new TestMessage {Id = id});

            await Task.Delay(50);
            
            serviceBus.Subscribe<TestMessage>(new NullMessageHandler());

            await Task.Delay(50);
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
            serviceBus.Subscribe<TestMessage>(messageHandler);

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
            receiveServiceBus.Subscribe<TestMessage>(messageHandler);

            var publishTasks = Enumerable.Range(0, messages).Select(i => sendServiceBus.Publish(new TestMessage {Sequence = i}));
            await Task.WhenAll(publishTasks);

            await WaitForSubscribers(sequences, messages);
         }
         
         Assert.That(sequences.Count, Is.EqualTo(messages));
         Assert.That(sequences.Select(c => c.Sequence).Distinct().Count, Is.EqualTo(messages));
      }

      [Test]
      public async Task messages_are_handled_concurrently()
      {
         const int messages = 100;
         
         var sequences = new ConcurrentBag<SlowJobMessage>();
         var messageHandler = new SlowJobHandler(sequences);

         long duration;
         
         using (var sendServiceBus = await CreateServiceBus())
         using (var receiveServiceBus = await CreateServiceBus())
         {
            var publishTasks = Enumerable.Range(0, messages).Select(i => sendServiceBus.Publish(new SlowJobMessage { Id = i, Duration = 500}));
            await Task.WhenAll(publishTasks);

            var stopwatch = Stopwatch.StartNew();
            receiveServiceBus.Subscribe<SlowJobMessage>(messageHandler);
            await WaitForSubscribers(sequences, messages);

            duration = stopwatch.ElapsedMilliseconds;
         }
         
         Assert.That(sequences.Count, Is.EqualTo(messages));
         Assert.That(sequences.Select(c => c.Id).Distinct().Count, Is.EqualTo(messages));
         Assert.That(duration, Is.InRange(500, 1000));
      }
      
      private static async Task WaitForSubscribers<T>(IReadOnlyCollection<T> messages, int expectedCount)
      {
         var j = 2;
         while (j < 20 && messages.Count < expectedCount)
         {
            await Task.Delay(j * j);
            j++;
         }
      }
   }
}