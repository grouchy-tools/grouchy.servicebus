namespace Grouchy.ServiceBus.Tests
{
   using System;
   using System.Collections.Generic;
   using System.Threading;
   using System.Threading.Tasks;
   using Grouchy.ServiceBus.Abstractions;
   using Microsoft.Extensions.DependencyInjection;
   using NUnit.Framework;

   public class DefaultMessageProcessorTests
   {
      private IServiceProvider _serviceProvider;
      private Dictionary<string, ResolveHandler> _instances;
      private DefaultMessageProcessor _testSubject;
      
      [SetUp]
      public void setup_before_each_test()
      {
         _instances = new Dictionary<string, ResolveHandler>();

         var services = new ServiceCollection();
         services.AddSingleton(_instances);
         services.AddScoped<IMessageHandler<ResolveCommand>, ResolveHandler>();

         _serviceProvider = services.BuildServiceProvider();
         _testSubject = new DefaultMessageProcessor(_serviceProvider);
      }

      [Test]
      public async Task resolving_inside_handler_returns_same_instance()
      {
         await _testSubject.ProcessAsync(new ResolveCommand { Key="foo"}, CancellationToken.None);
         
         Assert.That(_instances["resolved-inside-foo"], Is.SameAs(_instances["resolved-outside-foo"]));
      }

      [Test]
      public async Task subsequent_call_to_process_returns_different_instance()
      {
         await _testSubject.ProcessAsync(new ResolveCommand { Key="first"}, CancellationToken.None);

         await _testSubject.ProcessAsync(new ResolveCommand { Key="second"}, CancellationToken.None);

         Assert.That(_instances["resolved-inside-second"], Is.Not.SameAs(_instances["resolved-inside-first"]));
      }

      // TODO: Write 2 metrics for happy path
      // TODO: Write 2 metrics when throws exception

      private class ResolveCommand
      {
         public string Key { get; set; }
      }

      private class ResolveHandler : IMessageHandler<ResolveCommand>
      {
         private readonly IServiceProvider _serviceProvider;
         private readonly Dictionary<string, ResolveHandler> _instances;

         public ResolveHandler(
            IServiceProvider serviceProvider,
            Dictionary<string, ResolveHandler> instances)
         {
            _serviceProvider = serviceProvider;
            _instances = instances;
         }

         public Task HandleAsync(ResolveCommand message, CancellationToken cancellationToken)
         {
            _instances.Add($"resolved-outside-{message.Key}", this);
            _instances.Add($"resolved-inside-{message.Key}", (ResolveHandler)_serviceProvider.GetService<IMessageHandler<ResolveCommand>>());
            
            return Task.CompletedTask;
         }
      }
   }
}
