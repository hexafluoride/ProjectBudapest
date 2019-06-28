using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sopron.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sopron
{
    public class JsonConnection : IConnection
    {
        public event OnConnectionClosed ConnectionClosed;

        public TcpClient Client { get; set; }
        private JsonTextReader Reader { get; set; }
        private JsonTextWriter Writer { get; set; }
        private JsonSerializer Serializer { get; set; }

        public JsonConnection(TcpClient client)
        {
            Client = client;
        }

        public void Close() => Client?.Client?.Close();

        public void Initialize()
        {
            var stream = Client.GetStream();

            Reader = new JsonTextReader(new StreamReader(stream));
            Reader.SupportMultipleContent = true;

            Writer = new JsonTextWriter(new StreamWriter(stream) { AutoFlush = true });

            Serializer = new JsonSerializer();
            Serializer.Converters.Add(new SopronTypeJsonConverter());
            Serializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public async Task Send(object obj)
        {
            var jobj = JObject.FromObject(obj, Serializer);
                await jobj.WriteToAsync(Writer);
        }

        public async Task<object> Receive()
        {
            try
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
            catch (Exception ex)
            {
                if (ex is IOException || !Client.Connected)
                {
                    ConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(false));
                }
                else if (Client.Client.Poll(0, SelectMode.SelectRead))
                {
                    // if we haven't called .Close, Connected will always stay true
                    // this takes care of that
                    byte[] check = new byte[1];
                    if (Client.Client.Receive(check, SocketFlags.Peek) == 0)
                    {
                        ConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(false));
                    }
                }

                return null;
            }
        }
    }
}
