using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.Messages
{
    [SopronMessage]
    public class Signal
    {
        [JsonProperty("signal")]
        public SignalType Type { get; set; }
        public TimeSpan GracePeriod { get; set; }

        public Signal()
        {

        }

        public Signal(SignalType type)
        {
            Type = type;
            GracePeriod = TimeSpan.Zero;
        }

        public Signal(SignalType type, TimeSpan grace)
        {
            Type = type;
            GracePeriod = grace;
        }
    }

    public enum SignalType
    {
        Unknown,
        PreShutdown,
        PreRestart,
        Shutdown,
        Restart,
        Rehash
    }
}
