using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sopron.DataTypes;

namespace Bursa
{
    public class ConsoleMessageSource : IMessageSource
    {
        public async Task SendMessage(Message msg)
        {
            await Console.Out.WriteLineAsync(msg.Contents);
        }

        public async Task<Message> GetMessage()
        {
            Console.Write(">> ");
            var line = await Console.In.ReadLineAsync();

            var message = new Message()
            {
                Context = "CONTEXT_CONSOLE",
                Contents = line,
                RawContents = line,
                Time = DateTime.Now,
                Location = new Uri("console:///console"),
                User = new Uri("console:///user"),
                SelfIdentifier = new Uri("console:///console")
            };

            return message;
        }

        public void Initialize(JObject config)
        {
        }
    }
}
