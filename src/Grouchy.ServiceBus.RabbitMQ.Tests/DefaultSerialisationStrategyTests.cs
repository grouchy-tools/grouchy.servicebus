using System;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Grouchy.ServiceBus.RabbitMQ.Tests
{
   public class DefaultSerialisationStrategyTests
   {
      [Test]
      public void message_can_be_serialised_and_deserialised()
      {
         var testSubject = new DefaultSerialisationStrategy();

         var message = new SimpleMessage
         {
            Guid = Guid.NewGuid(),
            DateTime = DateTime.Now,
            Double = new Random().NextDouble()
         };
         
         var body = testSubject.Serialise(message);
         var deserialised = testSubject.Deserialise<SimpleMessage>(body);
         
         deserialised.Should().BeEquivalentTo(message);
      }

      [Test]
      public void message_is_serialised_as_expected()
      {
         var testSubject = new DefaultSerialisationStrategy();

         var message = new SimpleMessage
         {
            Guid = Guid.Parse("bf440c4c-cfd6-465f-8504-3e270f935c8f"),
            DateTime = DateTime.Parse("2019-02-10T20:59:00.4148926"),
            Double = 0.44058390820426119
         };
         
         var body = testSubject.Serialise(message);
         var serialisedMessage = Encoding.UTF8.GetString(body);

         Assert.That(serialisedMessage, Is.EqualTo("{\"guid\":\"bf440c4c-cfd6-465f-8504-3e270f935c8f\",\"dateTime\":\"2019-02-10T20:59:00.4148926\",\"double\":0.44058390820426119}"));
      }

      private class SimpleMessage
      {
         public Guid Guid { get; set; }
         
         public DateTime DateTime { get; set; }
         
         public double Double { get; set; }
      }
   }
}