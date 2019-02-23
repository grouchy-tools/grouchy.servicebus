namespace Grouchy.ServiceBus.InMemory
{
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;

   public class InMemoryServiceBus : IServiceBus
   {
      private readonly ConcurrentMessageQueues _queues;
      private readonly IMessageProcessor _messageProcessor;

      public InMemoryServiceBus(
         ConcurrentMessageQueues queues,
         IMessageProcessor messageProcessor)
      {
         _queues = queues;
         _messageProcessor = messageProcessor;
      }

      public Task Publish<TMessage>(TMessage message)
         where TMessage : class
      {
         var queue = _queues.GetQueue<TMessage>();

         queue.Add(message);

         return Task.CompletedTask;
      }

      public IMessageSubscription Subscribe<TMessage>()
         where TMessage : class
      {
         var queue = _queues.GetQueue<TMessage>();

         var cancellationTokenSource = new CancellationTokenSource();
         var cancellationToken = cancellationTokenSource.Token;

         Task.Run(()  =>
         {            
            while (!cancellationToken.IsCancellationRequested)
            {
               var message = queue.Take(cancellationToken);

               // TODO: Error handling
               Task.Run(() => _messageProcessor.ProcessAsync(message, cancellationToken), cancellationToken);
            }
         }, cancellationToken);

         return new InMemoryMessageSubscription(cancellationTokenSource);
      }

      // TODO: Tidy up
      public void Dispose()
      {
      }
   }
}