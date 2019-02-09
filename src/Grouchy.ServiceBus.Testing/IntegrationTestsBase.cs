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
      
      // TODO:
      // publish before subscribe
      // subscribe before publish
      
      [Test]
      public async Task multiple_messages_are_all_sent_and_received_in_same_bus()
      {
         const int messages = 100;
         
         var sequences = new ConcurrentBag<int>();
         var messageHandler = new TestMessageHandler(sequences);

         using (var serviceBus = await CreateServiceBus())
         {
            serviceBus.Subscribe<TestMessage, TestMessageHandler>(messageHandler);

            var publishTasks = Enumerable.Range(0, messages).Select(i => serviceBus.Publish(new TestMessage {Sequence = i}));
            await Task.WhenAll(publishTasks);

            var j = 0;
            while (j < 10 && sequences.Count < messages)
            {
               await Task.Delay(200);
               j++;
            }
         }
         
         Assert.That(sequences.Count, Is.EqualTo(messages));
         Assert.That(sequences.Distinct().Count, Is.EqualTo(messages));
      }

      [Test]
      public async Task multiple_messages_are_sent_from_and_received_by_different_bus()
      {
         const int messages = 100;
         
         var sequences = new ConcurrentBag<int>();
         var messageHandler = new TestMessageHandler(sequences);
         
         using (var sendServiceBus = await CreateServiceBus())
         using (var receiveServiceBus = await CreateServiceBus())
         {
            receiveServiceBus.Subscribe<TestMessage, TestMessageHandler>(messageHandler);

            var publishTasks = Enumerable.Range(0, messages).Select(i => sendServiceBus.Publish(new TestMessage {Sequence = i}));
            await Task.WhenAll(publishTasks);

            var j = 0;
            while (j < 10 && sequences.Count < messages)
            {
               await Task.Delay(200);
               j++;
            }
         }
         
         Assert.That(sequences.Count, Is.EqualTo(messages));
         Assert.That(sequences.Distinct().Count, Is.EqualTo(messages));
      }

      public class TestMessage
      {
         public int Sequence { get; set; }
      }

      public class TestMessageHandler : IMessageHandler<TestMessage>
      {
         private readonly ConcurrentBag<int> _sequences;

         public TestMessageHandler(ConcurrentBag<int> sequences)
         {
            _sequences = sequences;
         }

         public Task Handle(TestMessage message)
         {
            _sequences.Add(message.Sequence);

            return Task.CompletedTask;
         }
      }
   }
}