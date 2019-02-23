namespace Grouchy.ServiceBus.Testing.Handlers
{
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;
   using Grouchy.ServiceBus.Testing.Messages;

   public class NullMessageHandler : IMessageHandler<TestMessage>
   {
      public Task HandleAsync(TestMessage message, CancellationToken cancellationToken)
      {
         return Task.CompletedTask;
      }
   }
}