using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.Messages
{
    public abstract class Hello
    {
        [JsonProperty("humanname")]
        public string HumanReadableName { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Language { get; set; }
        public string License { get; set; }
        public List<string> Capabilities { get; set; }
    }
}
