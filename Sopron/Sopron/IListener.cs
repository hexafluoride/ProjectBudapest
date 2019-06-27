using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sopron
{
    public interface IListener
    {
        void Start();
        void Stop();

        Task<IConnection> Accept();
    }
}
