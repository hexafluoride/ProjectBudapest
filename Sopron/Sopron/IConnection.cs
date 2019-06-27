using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sopron
{
    public interface IConnection
    {
        void Initialize();
        Task<object> Receive();
        Task Send(object obj);
    }
}
