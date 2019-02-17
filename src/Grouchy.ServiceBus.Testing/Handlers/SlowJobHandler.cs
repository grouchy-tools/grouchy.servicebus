namespace Grouchy.ServiceBus.Testing.Handlers
{
   using System;
   using System.Collections.Concurrent;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;
   using Grouchy.ServiceBus.Testing.Messages;

   public class SlowJobHandler : IMessageHandler<SlowJobMessage>
   {
      private readonly ConcurrentBag<SlowJobMessage> _messages;

      public SlowJobHandler(ConcurrentBag<SlowJobMessage> messages)
      {
         _messages = messages;
      }

      public async Task Handle(SlowJobMessage message)
      {
         Console.WriteLine($"In {message.Id}");
         await Task.Delay(message.Duration);
         
         _messages.Add(message);
         Console.WriteLine($"Out {message.Id}");
      }
   }
}