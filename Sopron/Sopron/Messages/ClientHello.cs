using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.Messages
{
    [SopronMessage]
    public class ClientHello : Hello
    {
        public ClientHello()
        {

        }
    }
}
