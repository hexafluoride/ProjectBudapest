using Sopron;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bursa
{
    public class LocalListener : IListener
    {
        private Channel<LocalConnection> Connections = Channel.CreateUnbounded<LocalConnection>();

        public async Task Queue(LocalConnection connection)
        {
            await Connections.Writer.WriteAsync(connection);
        }

        public async Task<IConnection> Accept()
        {
            return await Connections.Reader.ReadAsync();
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
