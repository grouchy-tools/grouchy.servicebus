namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
   using NUnit.Framework;
   using Grouchy.ServiceBus.Abstractions;

   public class DefaultQueueNameStrategyTests
   {
      [Test]
      public void queue_name_of_simple_poco_is_type_name()
      {
         var testSubject = new DefaultQueueNameStrategy();

         var result = testSubject.GetQueueName(typeof(SimplePoco));
         
         Assert.That(result, Is.EqualTo("SimplePoco"));
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

      [MessageName("alternate-queue-name")]
      private class PocoWithQueueNameAttribute
      {
      }
   }
}
