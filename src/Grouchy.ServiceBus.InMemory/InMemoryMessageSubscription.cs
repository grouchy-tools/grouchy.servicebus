namespace Grouchy.ServiceBus.InMemory
{
   using System;
   using System.Threading;
   using Grouchy.ServiceBus.Abstractions;

   public class InMemoryMessageSubscription : IMessageSubscription
   {
      private readonly CancellationTokenSource _cancellationTokenSource;

      private bool _disposed = false;

      public InMemoryMessageSubscription(CancellationTokenSource cancellationTokenSource)
      {
         _cancellationTokenSource = cancellationTokenSource;
      }
      
      ~InMemoryMessageSubscription()
      {
         Dispose(false);         
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (_disposed) return;

         _cancellationTokenSource.Cancel();

         if (disposing)
         {
            _cancellationTokenSource.Dispose();
         }

         _disposed = true;
      }
   }
}