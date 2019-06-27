using Sopron;
using Sopron.DataTypes;
using Sopron.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bursa
{
    public class Server
    {
        public AsyncProcessor<IMessageSource, Message> MessageProcessor = new AsyncProcessor<IMessageSource, Message>();
        public AsyncProcessor<IListener, IConnection> ListenerProcessor = new AsyncProcessor<IListener, IConnection>();
        public AsyncProcessor<IConnection, object> SopronMessageProcessor = new AsyncProcessor<IConnection, object>();

        public List<BursaClient> Modules = new List<BursaClient>();
        public Dictionary<IConnection, BursaClient> Connections = new Dictionary<IConnection, BursaClient>();

        private HandlerDictionary<Type, MessageHandler> MessageHandlers = new HandlerDictionary<Type, MessageHandler>();
        private Dictionary<int, WaitingCommandCall> WaitingCalls = new Dictionary<int, WaitingCommandCall>();

        private Random Random = new Random();

        public ServerHello ServerHello = new ServerHello()
        {
            Capabilities = new List<string>() { "CAP_UPTIME" },
            HumanReadableName = "Bursa",
            Version = "0.1",
            Language = "C#",
            License = "GPLv3",
            Name = "bursa"
        };

        public Server()
        {
            ListenerProcessor.TaskComplete += HandleIncomingConnection;
            SopronMessageProcessor.TaskComplete += HandleSopronMessage;
            MessageProcessor.TaskComplete += HandleMessage;

            MessageProcessor.TaskRetriever = _ => (_ as IMessageSource).GetMessage();
            ListenerProcessor.TaskRetriever = _ => (_ as IListener).Accept();
            SopronMessageProcessor.TaskRetriever = _ => (_ as IConnection).Receive();

            MessageHandlers.Add(typeof(CommandResult), HandleCommandResult);
            MessageHandlers.Add(typeof(ClientHello), HandleClientHello);
        }

        private async Task HandleCommandResult(object sender, MessageHandlerEventArgs e)
        {
            var result = e.Message as CommandResult;

            if (string.IsNullOrEmpty(result.Output))
                return;

            var call = WaitingCalls[result.CallId];
            var reply = new Message()
            {
                Contents = result.Output,
                RawContents = result.Output,
                Time = DateTime.Now,
                User = call.Message.SelfIdentifier,
                Location = call.Message.Location,
                SelfIdentifier = call.Message.SelfIdentifier
            };

            WaitingCalls.Remove(result.CallId);

            await call.Source.SendMessage(reply);
        }

        private async Task HandleMessage(IMessageSource source, Message msg)
        {
            foreach (var pair in Modules.Select(m => new { Module = m, Commands = m.GetMatchingCommands(msg) }))
            {
                foreach (var command in pair.Commands)
                {
                    var notification = new CommandNotification()
                    {
                        CallId = Random.Next(),
                        Id = command.Id,
                        Message = msg
                    };

                    WaitingCalls[notification.CallId] = new WaitingCommandCall(notification, source);
                    pair.Module.Connection.Send(notification);
                }
            }
        }

        private async Task HandleClientHello(object sender, MessageHandlerEventArgs e)
        {
            Connections[e.Connection].ClientHello = e.Message as ClientHello;
        }

        private async Task HandleSopronMessage(IConnection connection, object message)
        {
            var type = message.GetType();

            if (MessageHandlers.ContainsKey(type))
            {
                var args = new MessageHandlerEventArgs(connection, message, type);

                foreach (var handler in MessageHandlers[type])
                {
                    await handler(this, args);
                }
            }

            await Connections[connection].ProcessMessage(message);
        }

        private async Task HandleIncomingConnection(IListener listener, IConnection connection)
        {
            var client = new BursaClient(connection);

            Modules.Add(client);
            Connections[connection] = client;
            SopronMessageProcessor.Add(connection);
            await connection.Send(ServerHello);
        }

        public void AddMessageSource(IMessageSource source) => MessageProcessor.Add(source);
        public void AddListener(IListener listener) => ListenerProcessor.Add(listener);

        public async Task StartListening()
        {
            while (true)
            {
                await ListenerProcessor.Process().ConfigureAwait(false);
            }
        }

        public async Task StartProcessing()
        {
            while (true)
            {
                await SopronMessageProcessor.Process().ConfigureAwait(false);
            }
        }

        public async Task StartHandlingMessages()
        {
            while (true)
            {
                await MessageProcessor.Process().ConfigureAwait(false);
            }
        }
    }

    public class WaitingCommandCall
    {
        public CommandNotification Notification { get; set; }
        public Message Message => Notification.Message;
        public int CallId => Notification.CallId;
        public IMessageSource Source { get; set; }

        public WaitingCommandCall(CommandNotification notification, IMessageSource source)
        {
            Notification = notification;
            Source = source;
        }
    }
}
