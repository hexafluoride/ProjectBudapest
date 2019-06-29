using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sopron.DataTypes;

namespace Bursa
{
    public class DummyMessageSource : IMessageSource
    {
        public long SentMessages { get; set; }
        public long ReceivedMessages { get; set; }

        public async Task<Message> GetMessage()
        {
            SentMessages++;
            return new Message()
            {
                Contents = ".test",
                RawContents = ".test",
                Context = "CONTEXT_DUMMY",
                Location = new Uri("dummy:///"),
                SelfIdentifier = new Uri("dummy:///"),
                User = new Uri("dummy:///"),
                Time = DateTime.UtcNow
            };
        }

        public void Initialize(JObject config)
        {
        }

        public Task SendMessage(Message msg)
        {
            ReceivedMessages++;
            return Task.CompletedTask;
        }
    }
}
