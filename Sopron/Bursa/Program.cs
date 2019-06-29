using Sopron;
using Sopron.Messages;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Bursa
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SopronTypeJsonConverter.InitializeTypes();
            MessageSourceRegistry.Initialize();

            var server = new Server();
            var listener = new JsonListener(IPAddress.Loopback, 3131);
            
            listener.Start();

            server.StartListening();
            server.StartProcessing();
            server.StartHandlingMessages();
            
            server.AddListener(listener);

            if (Directory.Exists("sources"))
            {
                var message_source_files = Directory.GetFiles("sources", "*.json");

                foreach(var file in message_source_files)
                {
                    var fs = File.OpenRead(file);
                    var obj = JToken.ReadFrom(new JsonTextReader(new StreamReader(fs)));
                    fs.Close();

                    if(obj is JArray)
                    {
                        foreach(JObject source_obj in obj as JArray)
                        {
                            server.AddMessageSource(MessageSourceRegistry.GetMessageSource(source_obj));
                        }
                    }
                    else
                    {
                        server.AddMessageSource(MessageSourceRegistry.GetMessageSource(obj as JObject));
                    }
                }
            }

            await Task.Delay(-1);
        }
    }
}
