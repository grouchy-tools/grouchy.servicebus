namespace Grouchy.ServiceBus.InMemory
{
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;

   public class InMemoryServiceBus : IServiceBus
   {
      private readonly IJobQueue _jobQueue;
      private readonly ConcurrentMessageQueues _queues;

      public InMemoryServiceBus(
         IJobQueue jobQueue,
         ConcurrentMessageQueues queues)
      {
         _jobQueue = jobQueue;
         _queues = queues;
      }

      public Task Publish<TMessage>(TMessage message)
         where TMessage : class
      {
         var queue = _queues.GetQueue<TMessage>();

         queue.Add(message);

         return Task.CompletedTask;
      }

      public IMessageSubscription Subscribe<TMessage>(IMessageHandler<TMessage> messageHandler)
         where TMessage : class
      {
         var queue = _queues.GetQueue<TMessage>();

         var cancellationTokenSource = new CancellationTokenSource();
         var cancellationToken = cancellationTokenSource.Token;

         IJob JobFactory(TMessage message) => new MessageHandlerJob<TMessage>(message, messageHandler);

         Task.Run(() =>
         {            
            while (!cancellationToken.IsCancellationRequested)
            {
               var message = queue.Take(cancellationToken);

               _jobQueue.Enqueue(JobFactory(message));
            }
         }, cancellationToken);

         return new InMemoryMessageSubscription(cancellationTokenSource);
      }

      // TODO: Tidy up
      public void Dispose()
      {
      }
      
      private class MessageHandlerJob<TMessage> : IJob
         where TMessage : class
      {
         private readonly TMessage _message;
         private readonly IMessageHandler<TMessage> _messageHandler;

         public MessageHandlerJob(TMessage message, IMessageHandler<TMessage> messageHandler)
         {
            _messageHandler = messageHandler;
            _message = message;
         }
         
         public async Task RunAsync(CancellationToken cancellationToken)
         {
            // TODO: Add cancellationToken to Handle method
            // TODO: Error handling
            await _messageHandler.Handle(_message);
         }
      }
   }
}