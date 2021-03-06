﻿namespace Grouchy.ServiceBus.Testing
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
   using Microsoft.Extensions.DependencyInjection;

   public abstract class IntegrationTestsBase
   {
      private IServiceProvider _serviceProvider;
      private ConcurrentBag<TestMessage> _testMessages;
      private ConcurrentBag<SlowJobMessage> _slowJobMessages;

      protected abstract Task<IServiceBus> CreateServiceBus(IServiceProvider sp);

      // TODO: Receive on different bus instances
      // TODO: Send vs Publish, Receive vs Subscribe
      // TODO: Handling unroutable messages https://www.rabbitmq.com/dotnet-api-guide.html
      // TODO: manual ack/nack

      [SetUp]
      public void setup_before_each_test_base()
      {
         var services = new ServiceCollection();

         _testMessages = new ConcurrentBag<TestMessage>();
         _slowJobMessages = new ConcurrentBag<SlowJobMessage>();

         services.AddSingleton(_testMessages);
         services.AddSingleton(_slowJobMessages);
         services.AddScoped<IMessageHandler<TestMessage>, TestMessageHandler>();
         services.AddScoped<IMessageHandler<SlowJobMessage>, SlowJobHandler>();
         
         _serviceProvider = services.BuildServiceProvider();
      }
      
      [Test]
      public async Task publish_before_subscribe()
      {
         var id = Guid.NewGuid().ToString().Substring(8);

         using (var serviceBus = await CreateServiceBus(_serviceProvider))
         {
            await serviceBus.Publish(new TestMessage {Id = id});
            
            serviceBus.Subscribe<TestMessage>();
            
            await WaitForSubscribers(_testMessages, 1);
         }
         
         Assert.That(_testMessages.Count, Is.EqualTo(1));
         Assert.That(_testMessages.Single().Id, Is.EqualTo(id));
      }

      [Test]
      public async Task subscribe_before_publish()
      {
         var id = Guid.NewGuid().ToString().Substring(8);
         
         using (var serviceBus = await CreateServiceBus(_serviceProvider))
         {
            serviceBus.Subscribe<TestMessage>();
            
            await serviceBus.Publish(new TestMessage {Id = id});
            
            await WaitForSubscribers(_testMessages, 1);
         }
         
         Assert.That(_testMessages.Count, Is.EqualTo(1));
         Assert.That(_testMessages.Single().Id, Is.EqualTo(id));
      }

      [Test]
      [Category("local-only")]
      public async Task dispose_subscription_before_publish()
      {
         var id = Guid.NewGuid().ToString().Substring(8);
         
         using (var serviceBus = await CreateServiceBus(_serviceProvider))
         {
            var subscription = serviceBus.Subscribe<TestMessage>();
            subscription.Dispose();
            
            await serviceBus.Publish(new TestMessage {Id = id});

            await Task.Delay(50);

            Assert.That(_testMessages.Count, Is.EqualTo(0));
            
            serviceBus.Subscribe<TestMessage>();

            await Task.Delay(50);
         }
         
         Assert.That(_testMessages.Count, Is.EqualTo(1));
      }

      [Test]
      public async Task multiple_messages_are_all_sent_and_received_in_same_bus()
      {
         const int messages = 100;
         
         using (var serviceBus = await CreateServiceBus(_serviceProvider))
         {
            serviceBus.Subscribe<TestMessage>();

            var publishTasks = Enumerable.Range(0, messages).Select(i => serviceBus.Publish(new TestMessage {Sequence = i}));
            await Task.WhenAll(publishTasks);

            await WaitForSubscribers(_testMessages, messages);
         }
         
         Assert.That(_testMessages.Count, Is.EqualTo(messages));
         Assert.That(_testMessages.Select(c => c.Sequence).Distinct().Count, Is.EqualTo(messages));
      }

      [Test]
      public async Task multiple_messages_are_sent_from_and_received_by_different_bus()
      {
         const int messages = 100;
         
         using (var sendServiceBus = await CreateServiceBus(_serviceProvider))
         using (var receiveServiceBus = await CreateServiceBus(_serviceProvider))
         {
            receiveServiceBus.Subscribe<TestMessage>();

            var publishTasks = Enumerable.Range(0, messages).Select(i => sendServiceBus.Publish(new TestMessage {Sequence = i}));
            await Task.WhenAll(publishTasks);

            await WaitForSubscribers(_testMessages, messages);
         }
         
         Assert.That(_testMessages.Count, Is.EqualTo(messages));
         Assert.That(_testMessages.Select(c => c.Sequence).Distinct().Count, Is.EqualTo(messages));
      }

      [Test]
      public async Task messages_are_handled_concurrently()
      {
         const int messages = 100;
         
         long duration;
         
         using (var sendServiceBus = await CreateServiceBus(_serviceProvider))
         using (var receiveServiceBus = await CreateServiceBus(_serviceProvider))
         {
            var publishTasks = Enumerable.Range(0, messages).Select(i => sendServiceBus.Publish(new SlowJobMessage { Id = i, Duration = 200}));
            await Task.WhenAll(publishTasks);

            var stopwatch = Stopwatch.StartNew();
            receiveServiceBus.Subscribe<SlowJobMessage>();
            await WaitForSubscribers(_slowJobMessages, messages);

            duration = stopwatch.ElapsedMilliseconds;
         }
         
         Assert.That(_slowJobMessages.Count, Is.EqualTo(messages));
         Assert.That(_slowJobMessages.Select(c => c.Id).Distinct().Count, Is.EqualTo(messages));
         // 20s if messages are processed sequentially, 10 concurrent consumers reduces this to 2s
         Assert.That(duration, Is.LessThan(3000));
      }

//      [Test]
//      public async Task foo()
//      {
//         using (var sendServiceBus = await CreateServiceBus(_serviceProvider))
//         using (var receiveServiceBus = await CreateServiceBus(_serviceProvider))
//         {
//            await sendServiceBus.Publish(new TestMessage { Id = "Y#71" });
//
//            receiveServiceBus.Subscribe<TestMessage>();
//         }
//      }
//      
      // TODO: send, nack, ack, check 0 remain
      
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