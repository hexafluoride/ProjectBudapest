using Newtonsoft.Json.Linq;
using Sopron.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bursa
{
    public interface IMessageSource
    {
        void Initialize(JObject config);
        Task SendMessage(Message msg);
        Task<Message> GetMessage();
    }
}
