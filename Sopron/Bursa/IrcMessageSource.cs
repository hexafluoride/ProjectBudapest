using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using IrcDotNet;
using Newtonsoft.Json.Linq;
using Sopron.DataTypes;

namespace Bursa
{
    public class IrcMessageSource : IMessageSource
    {
        public IrcServerConfig Config { get; set; }
        public StandardIrcClient Server { get; set; }

        private Channel<Message> Messages = Channel.CreateUnbounded<Message>();

        public IrcMessageSource()
        {

        }

        public void Initialize(JObject config)
        {
            Config = config.ToObject<IrcServerConfig>();
            Connect();
        }

        private void Connect()
        {
            Server = new StandardIrcClient();

            Server.RawMessageReceived += HandleRawMessage;
            Server.Connected += HandleServerConnected;
            Server.Registered += HandleRegistered;

            Server.Connect(Config.Server, Config.Port, Config.Ssl, new IrcUserRegistrationInfo()
            {
                NickName = Config.Nickname,
                RealName = Config.Nickname,
                UserName = Config.Nickname,
                Password = Config.Password
            });
        }

        private void HandleRegistered(object sender, EventArgs e)
        {
            if (Config.Autojoin?.Any() ?? false)
                Server.Channels.Join(Config.Autojoin);
        }

        private void HandleServerConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected");
        }

        private void HandleRawMessage(object sender, IrcRawMessageEventArgs e)
        {
            if (e.Message.Command == "PRIVMSG")
            {
                var source = e.Message.Parameters[0];
                var contents = e.Message.Parameters[1];
                var prefix = e.Message.Prefix;

                var message = new Message()
                {
                    Location = new Uri($"irc://{Config.Server}:{Config.Port}/{source}"),
                    User = new Uri($"irc://{Config.Server}:{Config.Port}/{prefix.Split('!')[0]}"),
                    Contents = contents,
                    RawContents = e.RawContent,
                    Time = DateTime.UtcNow,
                    Context = source.StartsWith("#") ? "CONTEXT_CHANNEL" : "CONTEXT_PRIVATE",
                    SelfIdentifier = new Uri($"irc://{Config.Server}:{Config.Port}/{Server.LocalUser.NickName}")
                };

                Messages.Writer.WriteAsync(message);
            }
        }

        public async Task SendMessage(Message msg)
        {
            var target = "";

            if (!string.IsNullOrEmpty(msg.Location.Fragment))
                target = msg.Location.Fragment;
            else
                target = msg.Location.AbsolutePath.Substring(1);

            Server.LocalUser.SendMessage(target, msg.Contents);
        }

        public async Task<Message> GetMessage()
        {
            return await Messages.Reader.ReadAsync();
        }
    }

    public class IrcServerConfig
    {
        public string Name { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string NickServPassword { get; set; }
        public string Owner { get; set; }
        public bool NickServ { get; set; }
        public bool Ssl { get; set; }
        public bool ZncLogin { get; set; }
        public bool Delay { get; set; }
        public string ZncPassword { get; set; }
        public string ZncUsername { get; set; }
        public string ZncNetwork { get; set; }
        public List<string> Autojoin { get; set; }

        public IrcServerConfig()
        {

        }
    }
}
