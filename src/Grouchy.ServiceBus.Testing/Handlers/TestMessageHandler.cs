namespace Grouchy.ServiceBus.Testing.Handlers
{
   using System;
   using System.Collections.Concurrent;
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;
   using Grouchy.ServiceBus.Testing.Messages;

   public class TestMessageHandler : IAsyncMessageHandler<TestMessage>
   {
      private readonly ConcurrentBag<TestMessage> _messages;

      public TestMessageHandler(ConcurrentBag<TestMessage> messages)
      {
         _messages = messages;
      }

      public Task HandleAsync(TestMessage message, CancellationToken cancellationToken)
      {
         _messages.Add(message);

         return Task.CompletedTask;
      }
   }

   public class FailingTestMessageHandler : IAsyncMessageHandler<TestMessage>
   {
      public Task HandleAsync(TestMessage message, CancellationToken cancellationToken)
      {
         throw new Exception(message.Id);
      }
   }
}