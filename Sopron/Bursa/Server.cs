using Sopron;
using Sopron.DataTypes;
using Sopron.Messages;
using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<int, WaitingCommandCall> WaitingCalls = new ConcurrentDictionary<int, WaitingCommandCall>();

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
            MessageHandlers.Add(typeof(ListModules), HandleListModules);
            MessageHandlers.Add(typeof(GetModuleInfo), HandleInfoRequest);
        }

        private async Task HandleInfoRequest(object sender, MessageHandlerEventArgs e)
        {
            var name = (e.Message as GetModuleInfo).ModuleName;

            var module = Modules.FirstOrDefault(m => m.ClientHello.Name == name);
            var info = new ModuleInfo();

            if (module != null)
            {
                info.ClientHello = module.ClientHello;
                info.Uptime = DateTime.UtcNow - module.Started;
                info.Health = ModuleHealth.Healthy;
            }

            e.Connection.Send(info);
        }

        private async Task HandleListModules(object sender, MessageHandlerEventArgs e)
        {
            var response = new ModuleList()
            {
                Modules = Modules.Select(m => m.ClientHello.Name).ToList()
            };

            e.Connection.Send(response);
            Console.WriteLine("hi");
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

            WaitingCalls.Remove(result.CallId, out WaitingCommandCall c);

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
                        Trigger = command.Triggers.First(t => t.Matches(msg.Contents)),
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
            if (message == null)
                return;

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

            connection.ConnectionClosed += HandleConnectionClosed;

            await connection.Send(ServerHello);
        }

        private void HandleConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            Console.WriteLine($"Purging {sender}");
            var connection = sender as IConnection;

            SopronMessageProcessor.Remove(connection);
            Modules.Remove(Connections[connection]);
            Connections.Remove(connection);
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
