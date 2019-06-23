using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.DataTypes
{
    [SopronDataType]
    public class Message
    {
        [JsonProperty("at")]
        public DateTime Time { get; set; }
        public Uri Source { get; set; }
        public Uri User { get; set; }
        [JsonProperty("message")]
        public string Contents { get; set; }
        [JsonProperty("raw")]
        public string RawContents { get; set; }

        public Message()
        {

        }
    }
}
