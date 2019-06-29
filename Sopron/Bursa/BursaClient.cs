using Sopron;
using Sopron.DataTypes;
using Sopron.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bursa
{
    public class BursaClient
    {
        public IConnection Connection { get; set; }
        public ClientHello ClientHello { get; set; }

        public Dictionary<string, Command> CommandsDictionary = new Dictionary<string, Command>();
        public List<Command> Commands = new List<Command>();
        private HandlerDictionary<Type, MessageHandler> MessageHandlers = new HandlerDictionary<Type, MessageHandler>();

        public DateTime Started { get; set; }

        public BursaClient(IConnection connection)
        {
            Connection = connection;
            MessageHandlers.Add(typeof(RegisterCommand), RegisterCommandHandler);
            Started = DateTime.UtcNow;
        }

        public IEnumerable<Command> GetMatchingCommands(Message message)
        {
            return Commands.Where(c => c.Triggers.Any(t => t.Matches(message.Contents)));
        }

        public async Task RegisterCommandHandler(object sender, MessageHandlerEventArgs e)
        {
            var command = (e.Message as RegisterCommand).Command;
            CommandsDictionary[command.Id] = command;
            Commands.Add(command);
        }

        public async Task ProcessMessages()
        {
            while (true)
                await ProcessMessage(await Connection.Receive());
        }

        public async Task ProcessMessage(object message)
        {
            if (message == null)
                return;

            var type = message.GetType();

            if (MessageHandlers.ContainsKey(type))
            {
                var args = new MessageHandlerEventArgs(Connection, message, type);

                foreach (var handler in MessageHandlers[type])
                {
                    await handler(this, args);
                }
            }
        }
    }
}
