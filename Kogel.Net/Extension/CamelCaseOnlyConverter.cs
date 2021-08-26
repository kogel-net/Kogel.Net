extern alias Kogel_Http;

using Kogel_Http::Newtonsoft.Json;
using Kogel_Http::Newtonsoft.Json.Linq;
using Kogel_Http::Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.Extension
{
    /// <summary>
    /// JsonConvert转换兼容
    /// </summary>
    public class CamelCaseOnlyConverter : JsonConverter
    {
        public static JsonSerializerSettings Settings => GetSettings();
        private static JsonSerializerSettings GetSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new CamelCaseOnlyConverter() },
            };
            return settings;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var token = (JObject)JToken.Load(reader);
            var isCamelCased = true;
            WalkNode(token, null,
            t =>
            {
                var nameFirstChar = t.Name[0].ToString();
                if (!nameFirstChar.Equals(nameFirstChar.ToLower(),
                    StringComparison.CurrentCulture))
                {
                    isCamelCased = false;
                    return;
                }
            });

            if (!isCamelCased) return null;

            return token.ToObject(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            JObject o = (JObject)JToken.FromObject(value);
            o.WriteTo(writer);
        }

        private static void WalkNode(JToken node,
                                Action<JObject> objectAction = null,
                                Action<JProperty> propertyAction = null)
        {
            if (node.Type == JTokenType.Object)
            {
                if (objectAction != null) objectAction((JObject)node);
                foreach (JProperty child in node.Children<JProperty>())
                {
                    if (propertyAction != null) propertyAction(child);
                    WalkNode(child.Value, objectAction, propertyAction);
                }
            }
            else if (node.Type == JTokenType.Array)
                foreach (JToken child in node.Children())
                    WalkNode(child, objectAction, propertyAction);
        }

        
    }
}
