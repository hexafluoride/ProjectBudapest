using Sopron;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Bursa
{
    class Program
    {
        static void Main(string[] args)
        {
            SopronTypeJsonConverter.InitializeTypes();
            ActualMain();
            Console.ReadLine();
        }

        static async void ActualMain()
        {
            var listener = new TcpListener(IPAddress.Loopback, 3131);
            listener.Start();

            var client = listener.AcceptTcpClient();
            var conn = new JsonConnection(client);

            while (true)
            {
                var msg = await conn.Receive();
                Console.WriteLine(msg);
            }
        }
    }
}
