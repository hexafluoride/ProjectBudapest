using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            return obj.ToObject(type);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = JObject.FromObject(value) as JObject;
            obj["type"] = value.GetType().Name;
            obj.WriteTo(writer);
        }
    }
}
