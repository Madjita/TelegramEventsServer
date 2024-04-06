using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;

namespace Helpers
{
    public static class JsonHelper
    {
        public static string SerializeObject(this object o, Formatting formatting, JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                return JsonConvert.SerializeObject(o, formatting);
            }
            else
            {
                return JsonConvert.SerializeObject(o, formatting, settings);
            }
        }

        public static string Serialize<T>(T toSerialize, Formatting formating = Formatting.None, JsonSerializerSettings settings = null)
        {

            string resp = JsonConvert.SerializeObject(toSerialize, formating, settings);
            return resp;
        }
         public static bool TryDeserialize<T>(this string objectData, out T result, out string error)
        {
            error = null;
            result = default(T);
            try
            {
                result = objectData.Deserialize<T>();
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
            return result is not null;
        }

        public static string SerializeToJsonWithNonQuotedNames(this object obj)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            StringWriter stringWriter = new StringWriter();

            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonTextWriter.QuoteName = false;

                jsonSerializer.Serialize(jsonTextWriter, obj);
            }

            return stringWriter.ToString();
        }

        public static bool IsValidJson(string jsonString)
        {
            //Если не спарсится то выкинет исключение, которое уйдет на стек вызова выше...
            JToken.Parse(jsonString);
            return true;
        }
        
        public static T Deserialize<T>(this string objectData)
        {
            return JsonConvert.DeserializeObject<T>(objectData);
        }

        public static T Deserialize<T>(this string objectData, params JsonConverter[] converters)
        {
            return JsonConvert.DeserializeObject<T>(objectData, converters);
        }

        public static T Deserialize<T>(this string objectData, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(objectData, settings);
        }

        public static T Deserialize<T>(this byte[] data, Encoding encoding = null)
        {
            var enc = encoding ?? Encoding.UTF8;
            string jsonString = enc.GetString(data);

            return jsonString.Deserialize<T>();
        }

        public static object DeserializeObject(this string objectData, Type objType)
        {
            return JsonConvert.DeserializeObject(objectData, objType);
        }

        public static T JsonSelectToken<T>(this string json, string name)
        {
            JObject data = JObject.Parse(json);

            var version = data.SelectToken(name, false) ?? data.SelectToken(name.FirstCharToUpper(), false);
            if (version != null)
                return version.ToObject<T>();

            return default(T);
        }

        /// <summary>
        /// Парсинг без преобразования формата времни из строк
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T JsonSelectTokenByText<T>(this string json, string name)
        {
            JObject data = JObject.Parse(json);

            var version = data.SelectToken(name, false) ?? data.SelectToken(name.FirstCharToUpper(), false);

            if (version != null)
                return version.ToString().Deserialize<T>();

            return default(T);
        }

        /// <summary>
        /// Возвращает ту же строку с первой буквой в верхнем регистре
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FirstCharToUpper(this string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;
            return input.First().ToString().ToUpper() + input.Substring(1);
        }


        /// <summary>
        /// Возвращает ту же строку с первой буквой в нижнем регистре
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FirstCharToLower(this string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;
            return input.First().ToString().ToLower() + input.Substring(1);
        }


        public class DecimalJsonConverter : JsonConverter
        {

            public DecimalJsonConverter()
            {
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(decimal) || objectType == typeof(float) || objectType == typeof(double));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteRawValue(string.Format(CultureInfo.InvariantCulture, "{0:0.##############}", value));
            }
        }
    }
}


