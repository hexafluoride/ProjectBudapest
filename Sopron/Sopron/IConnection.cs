using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sopron
{
    public delegate void OnConnectionClosed(object sender, ConnectionClosedEventArgs e);

    public interface IConnection
    {
        event OnConnectionClosed ConnectionClosed;
        void Initialize();
        void Close();

        Task<object> Receive();
        Task Send(object obj);
    }

    public class ConnectionClosedEventArgs : EventArgs
    {
        public bool Graceful { get; set; }

        public ConnectionClosedEventArgs(bool graceful)
        {
            Graceful = graceful;
        }
    }
}
