using Sopron;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bursa
{
    public class LocalConnection : IConnection
    {
        public event OnConnectionClosed ConnectionClosed;
        public ChannelWriter<object> Writer { get; set; }
        public ChannelReader<object> Reader { get; set; }

        public LocalConnection(ChannelReader<object> reader, ChannelWriter<object> writer)
        {
            Writer = writer;
            Reader = reader;
        }

        public static LocalConnection[] CreatePair()
        {
            var c1 = Channel.CreateUnbounded<object>();
            var c2 = Channel.CreateUnbounded<object>();

            return new LocalConnection[]
            {
                new LocalConnection(c1.Reader, c2.Writer),
                new LocalConnection(c2.Reader, c1.Writer)
            };
        }

        public void Close()
        {
        }

        public void Initialize()
        {
        }

        public async Task<object> Receive()
        {
            return await Reader.ReadAsync();
        }

        public async Task Send(object obj)
        {
            await Writer.WriteAsync(obj);
        }
    }
}
