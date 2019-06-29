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
        public ClientHello ClientHello { get; set; }
    }

    [SopronMessage]
    public class GetModuleInfo
    {
        public string ModuleName { get; set; }
        public GetModuleInfo(string name) => ModuleName = name;

        public GetModuleInfo() { }
    }

    public enum ModuleHealth
    {
        Unknown,
        Healthy,
        Degraded
    }
}
