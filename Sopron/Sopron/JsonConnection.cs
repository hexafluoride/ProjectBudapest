using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sopron
{
    public class JsonConnection
    {
        public TcpClient Client { get; set; }
        private JsonTextReader Reader { get; set; }
        private JsonSerializer Serializer { get; set; }

        public JsonConnection(TcpClient client)
        {
            Client = client;
            Reader = new JsonTextReader(new StreamReader(Client.GetStream()));
            Reader.SupportMultipleContent = true;
            Serializer = new JsonSerializer();
            Serializer.Converters.Add(new SopronTypeJsonConverter());
        }

        public async Task<object> Receive()
        {
            if (Reader.TokenType == JsonToken.EndObject)
                await Reader.ReadAsync();

            var obj = (await JToken.ReadFromAsync(Reader)) as JObject;

            if (!obj.ContainsKey("type"))
                return null;

            var type = obj.Value<string>("type");

            if (!SopronTypeJsonConverter.TypeMap.ContainsKey(type))
                return null;

            return obj.ToObject(SopronTypeJsonConverter.TypeMap[type], Serializer);
        }
    }
}
