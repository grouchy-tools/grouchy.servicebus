namespace Grouchy.ServiceBus.Testing.Handlers
{
   using System.Collections.Concurrent;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;
   using Grouchy.ServiceBus.Testing.Messages;

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
}