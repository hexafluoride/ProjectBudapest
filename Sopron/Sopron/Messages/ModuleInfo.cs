using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.Messages
{
    [SopronMessage]
    public class ModuleInfo
    {
        public TimeSpan Uptime { get; set; }
        public ModuleHealth Health { get; set; }
        public DateTime Rehashed { get; set; }
    }

    public enum ModuleHealth
    {
        Unknown,
        Healthy,
        Degraded
    }
}
