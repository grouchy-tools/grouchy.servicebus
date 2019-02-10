using NUnit.Framework;
using Grouchy.ServiceBus.Abstractions;

namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
   public class DefaultQueueNameStrategyTests
   {
      [Test]
      public void queue_name_of_simple_poco_is_type_name()
      {
         var testSubject = new DefaultQueueNameStrategy();

         var result = testSubject.GetQueueName(typeof(SimplePoco));
         
         Assert.That(result, Is.EqualTo("Grouchy.ServiceBus.RabbitMQ.Tests.DefaultQueueNameStrategyTests+SimplePoco"));
      }

      [Test]
      public void queue_name_of_poco_with_queue_name_attribute_is_attributes_name_property()
      {
         var testSubject = new DefaultQueueNameStrategy();

         var result = testSubject.GetQueueName(typeof(PocoWithQueueNameAttribute));
         
         Assert.That(result, Is.EqualTo("alternate-queue-name"));
      }

      private class SimplePoco
      {
      }

      [QueueName("alternate-queue-name")]
      private class PocoWithQueueNameAttribute
      {
      }
   }
}
