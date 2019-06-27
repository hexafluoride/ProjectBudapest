using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sopron
{
    public class JsonListener : IListener
    {
        private TcpListener Listener { get; set; }

        public JsonListener(IPAddress addr, int port)
        {
            Listener = new TcpListener(addr, port);
        }

        public JsonListener(int port) :
            this(IPAddress.Loopback, port)
        {

        }

        public void Start()
        {
            Listener.Start();
        }

        public void Stop()
        {
            Listener.Stop();
        }

        public async Task<IConnection> Accept()
        {
            var conn = new JsonConnection(await Listener.AcceptTcpClientAsync());
            conn.Initialize();
            return conn;
        }
    }
}
