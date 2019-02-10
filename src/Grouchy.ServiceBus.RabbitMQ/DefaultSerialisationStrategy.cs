using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Grouchy.ServiceBus.RabbitMQ
{
   public class DefaultSerialisationStrategy : ISerialisationStrategy
   {
      private readonly JsonSerializerSettings _serialiserSettings;

      public DefaultSerialisationStrategy()
      {
         _serialiserSettings = new JsonSerializerSettings
         {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
         };
      }
      
      public byte[] Serialise<TMessage>(TMessage message)
      {
         var json = JsonConvert.SerializeObject(message, Formatting.None, _serialiserSettings);
         var body = Encoding.UTF8.GetBytes(json);
         return body;
      }

      public TMessage Deserialise<TMessage>(byte[] body)
      {
         var json = Encoding.UTF8.GetString(body);
         var message = JsonConvert.DeserializeObject<TMessage>(json);
         return message;
      }
   }
}
