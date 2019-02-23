namespace Grouchy.ServiceBus.Testing.Handlers
{
   using System.Collections.Concurrent;
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;
   using Grouchy.ServiceBus.Testing.Messages;

   public class SlowJobHandler : IAsyncMessageHandler<SlowJobMessage>
   {
      private readonly ConcurrentBag<SlowJobMessage> _messages;

      public SlowJobHandler(ConcurrentBag<SlowJobMessage> messages)
      {
         _messages = messages;
      }

      public async Task HandleAsync(SlowJobMessage message, CancellationToken cancellationToken)
      {
         await Task.Delay(message.Duration);
         
         _messages.Add(message);
      }
   }
}