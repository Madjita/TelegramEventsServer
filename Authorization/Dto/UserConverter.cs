using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBase.Entities.Entities_DBContext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;


namespace Authorization.Dto
{
    public class UserConverter : JsonConverter<User>
    {
        public override User ReadJson(JsonReader reader, Type objectType, User existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            User user = new User();
            // Десериализуйте свойства пользователя из jsonObject
            // Например:
           // user.Id = jsonObject.Value<string>("Id");
           // user.Name = jsonObject.Value<string>("Name");
            // И так далее
            return user;
        }

        public override void WriteJson(JsonWriter writer, User value, JsonSerializer serializer)
        {
            var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(typeof(User));
            var jsonObject = new JObject();
            foreach (var property in contract.Properties)
            {
                var propertyValue = property.ValueProvider.GetValue(value);
                jsonObject.Add(property.PropertyName, JToken.FromObject(propertyValue, serializer));
            }
            jsonObject.WriteTo(writer);
        }
    }
}
