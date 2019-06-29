using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bursa
{
    public class MessageSourceRegistry
    {
        public static Dictionary<string, Type> MessageSources = new Dictionary<string, Type>();

        public static void Initialize() => Initialize(Assembly.GetExecutingAssembly());
        public static void Initialize(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var message_sources = types.Where(t => t.GetInterface("IMessageSource") != null);

            foreach (var source_type in message_sources)
            {
                var name = source_type.Name;
                if (name.EndsWith("MessageSource"))
                    name = name.Substring(0, name.Length - "MessageSource".Length);

                name = name.ToLower();

                MessageSources[name] = source_type;
            }
        }

        public static IMessageSource GetMessageSource(string name)
        {
            name = name.ToLower();

            if (!MessageSources.ContainsKey(name))
                return null;

            return Activator.CreateInstance(MessageSources[name]) as IMessageSource;
        }

        public static IMessageSource GetMessageSource(JObject obj)
        {
            var type = obj.Value<string>("type");
            var config = obj.ContainsKey("config") ? obj["config"] as JObject : null;

            var source = MessageSourceRegistry.GetMessageSource(type);
            source.Initialize(config);

            return source;
        }
    }
}
