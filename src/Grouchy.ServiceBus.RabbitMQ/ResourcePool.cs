namespace Grouchy.ServiceBus.RabbitMQ
{
   using System;
   using System.Collections.Concurrent;
   using System.Linq;
   using System.Threading;
   using System.Threading.Tasks;

   public class ResourcePool<TResource>
   {
      private readonly Func<TResource> _resourceFactory;
      private readonly SemaphoreSlim _semaphore;
      private readonly ConcurrentBag<TResource> _resources;

      public ResourcePool(int size, Func<TResource> resourceFactory)
      {
         _resourceFactory = resourceFactory;
         _semaphore = new SemaphoreSlim(size);
         _resources = new ConcurrentBag<TResource>();
      }
      
      public async Task<IResource<TResource>> AllocateAsync()
      {
         // Wait until resources are available
         await _semaphore.WaitAsync();

         if (_resources.TryTake(out var resource))
         {
            // Use pooled resource
            return new Resource(this, resource);
         }

         // Create another resource
         return new Resource(this, _resourceFactory());
      }

      private void ReturnResource(TResource resource)
      {
         _resources.Add(resource);
         _semaphore.Release();
      }
      
      private class Resource : IResource<TResource>
      {
         private readonly ResourcePool<TResource> _resourcePool;

         private bool _isDisposed;
         
         public Resource(ResourcePool<TResource> resourcePool, TResource value)
         {
            _resourcePool = resourcePool;
            Value = value;
         }
      
         public TResource Value { get; }

         ~Resource()
         {
            Dispose(false);
         }
         
         public void Dispose()
         {
            Dispose(true);
            GC.SuppressFinalize(this);
         }

         private void Dispose(bool isDisposing)
         {
            if (_isDisposed) return;

            _resourcePool.ReturnResource(Value);

            _isDisposed = true;
         }
      }
   }

   public interface IResource<TResource> : IDisposable
   {
      TResource Value { get; }
   }
}