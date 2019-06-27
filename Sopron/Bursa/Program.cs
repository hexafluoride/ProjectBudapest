using Sopron;
using Sopron.Messages;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Bursa
{
    class DummyIntTaskProvider
    {
        public int Value { get; set; }

        public DummyIntTaskProvider(int t)
        {
            Value = t;
        }

        public async Task<int> GetTask()
        {
            await Task.Delay(300);
            return Value++;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            SopronTypeJsonConverter.InitializeTypes();

            var server = new Server();
            var listener = new JsonListener(IPAddress.Loopback, 3131);
            
            listener.Start();

            server.StartListening();
            server.StartProcessing();
            server.StartHandlingMessages();

            server.AddMessageSource(new ConsoleMessageSource());
            server.AddListener(listener);

            // test code
            var test = new TestModule();

            await test.Connect("127.0.0.1", 3131);
            await test.Handshake();

            test.HandleMessages();

            await Task.Delay(-1);
        }
    }
}
