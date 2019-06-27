using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Sopron.DataTypes;
using Sopron.Messages;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sopron
{
    public class SopronTypeJsonConverter : JsonConverter
    {
        internal static Dictionary<string, Type> TypeMap = new Dictionary<string, Type>();
        internal static HashSet<Type> ConvertibleTypes = new HashSet<Type>();

        public static void InitializeTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var type in types)
            {
                if (type.GetCustomAttribute(typeof(SopronDataTypeAttribute)) != null ||
                    type.GetCustomAttribute(typeof(SopronMessageAttribute)) != null)
                {
                    TypeMap[type.Name] = type;
                    ConvertibleTypes.Add(type);
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return ConvertibleTypes.Contains(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JObject.ReadFrom(reader);

            if (!(token is JObject))
                return null;

            var obj = token as JObject;

            if (!obj.ContainsKey("type"))
                return null;

            var type_name = obj.Value<string>("type");

            if (!TypeMap.ContainsKey(type_name))
                return null;

            var type = TypeMap[type_name];
            existingValue = existingValue ?? serializer.ContractResolver.ResolveContract(type).DefaultCreator();
            serializer.Populate(new JTokenReader(obj), existingValue);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JsonObjectContract contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(value.GetType().Name);
            foreach (var property in contract.Properties)
            {
                if (property.Ignored)
                    continue;

                writer.WritePropertyName(property.PropertyName);
                serializer.Serialize(writer, property.ValueProvider.GetValue(value));
                //writer.WriteValue(property.ValueProvider.GetValue(value));
            }
            writer.WriteEndObject();

            //var obj = JObject.FromObject(value, serializer);
            //obj["type"] = value.GetType().Name;
            //obj.WriteTo(writer);
        }
    }
}
