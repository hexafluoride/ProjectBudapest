using Sopron;
using Sopron.DataTypes;
using Sopron.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bursa
{
    public delegate void CommandHandler(object sender, CommandHandlerEventArgs e);
    public delegate object ReturningCommandHandler(object sender, CommandHandlerEventArgs e);

    public delegate Task MessageHandler(object sender, MessageHandlerEventArgs e);

    public abstract class BursaModule
    {
        public string HumanReadableName => "Default Bursa Module";
        public string Name => "default-module";
        public string Version => "1.0";
        public string License => "MIT";

        public List<string> Capabilities = new List<string>()
        {
            "CAP_UPTIME"
        };

        private ServerHello ServerHello { get; set; }

        public JsonConnection Connection { get; set; }
        public Dictionary<string, Command> Commands = new Dictionary<string, Command>();
        public Dictionary<string, ReturningCommandHandler> CommandHandlers = new Dictionary<string, ReturningCommandHandler>();

        private HandlerDictionary<Type, MessageHandler> MessageHandlers = new HandlerDictionary<Type, MessageHandler>();

        private ManualResetEvent Connected = new ManualResetEvent(false);
        private string Hostname { get; set; }
        private int Port { get; set; }

        public BursaModule()
        {
            Initialize();

            MessageHandlers.Add(typeof(CommandNotification), HandleCommand);
        }

        public void Initialize()
        {
            // find all methods in current class with BursaCommand attribute
            var current_class_type = GetType();
            var methods = current_class_type.GetMethods();

            foreach(var method in methods)
            {
                var command_attribs = method.GetCustomAttributes(typeof(BursaCommandAttribute), true);
                var documentation_attribs = method.GetCustomAttributes(typeof(BursaDocumentationAttribute), true);

                if (!command_attribs.Any())
                    continue;
                
                var command_attrib = command_attribs.Last() as BursaCommandAttribute;

                var id_index = 1;
                var unused_id = command_attrib.Name;

                while (Commands.ContainsKey(unused_id))
                    unused_id = $"{unused_id}-{id_index++}";

                var command = command_attrib.ToCommand(unused_id);

                foreach (var attrib in documentation_attribs)
                    command.Documentation = (attrib as BursaDocumentationAttribute).ToDocumentation(command.Documentation);
                
                Commands[unused_id] = command;
                ReturningCommandHandler method_delegate;

                if (method.ReturnType == null)
                {
                    var temp_delegate = method.CreateDelegate(typeof(CommandHandler), this) as CommandHandler;
                    method_delegate = (s, e) => { temp_delegate(s, e); return null; };
                }
                else
                    method_delegate = method.CreateDelegate(typeof(ReturningCommandHandler), this) as ReturningCommandHandler;

                CommandHandlers[unused_id] = method_delegate;
            }
        }

        public async Task Connect(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;

            var client = new TcpClient();
            await client.ConnectAsync(hostname, port);
            Connection = new JsonConnection(client);
            Connection.Initialize();

            Connection.ConnectionClosed += HandleConnectionClosed;
        }

        private void HandleConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            Connected.Reset();

            new Thread((ThreadStart)async delegate
            {
                // attempt to reconnect
                while (!Connected.WaitOne(0))
                {
                    try { Connection.Client.Close(); } catch { }
                    //Thread.Sleep(5000);
                    try
                    {
                        await Connect(Hostname, Port);
                        await Handshake();
                    }
                    catch (Exception ex)
                    {

                    }
                }

                Console.WriteLine("Reconnected");
            }).Start();
        }

        public async Task Handshake()
        {
            var client_hello = new ClientHello()
            {
                HumanReadableName = HumanReadableName,
                Name = Name,
                Version = Version,
                License = License,
                Capabilities = Capabilities.ToList()
            };

            await Connection.Send(client_hello);
            var server_hello = await Connection.Receive();

            if (!(server_hello is ServerHello))
                throw new Exception($"Expected ServerHello, received {server_hello?.GetType()}");

            await RegisterCommands();
            Connected.Set();
        }

        private async Task RegisterCommands()
        {
            foreach (var command in Commands)
                await Connection.Send(new RegisterCommand() { Command = command.Value });
        }

        private async Task HandleCommand(object sender, MessageHandlerEventArgs e)
        {
            var msg = e.Message as CommandNotification;

            if (!CommandHandlers.ContainsKey(msg.Id))
                return;

            var ret = CommandHandlers[msg.Id](this, new CommandHandlerEventArgs(msg));
            await Connection.Send(new CommandResult() { CallId = msg.CallId, Output = (string)ret });
        }

        public async Task HandleMessages()
        {
            while(true)
            {
                Connected.WaitOne();
                await HandleMessage();
            }
        }

        public async Task HandleMessage()
        {
            var message = await Connection.Receive();

            if (message == null)
                return;

            var type = message.GetType();

            if(MessageHandlers.ContainsKey(type))
            {
                var args = new MessageHandlerEventArgs(Connection, message, type);

                foreach(var handler in MessageHandlers[type])
                {
                    await handler(this, args);
                }
            }
        }
    }

    public class CommandHandlerEventArgs : EventArgs
    {
        public CommandNotification Notification { get; set; }
        public string Id => Notification.Id;
        public Message Message => Notification.Message;
        public string Context => Message.Context;

        public string Contents => Message.Contents;
        public Uri Source => Message.Location;
        public Uri User => Message.User;

        public CommandHandlerEventArgs(CommandNotification notification)
        {
            Notification = notification;
        }
    }

    public class MessageHandlerEventArgs : EventArgs
    {
        public IConnection Connection { get; set; }
        public object Message { get; set; }
        public Type MessageType { get; set; }

        public MessageHandlerEventArgs(IConnection connection, object message, Type type)
        {
            Connection = connection;
            Message = message;
            MessageType = type;
        }
    }
}
