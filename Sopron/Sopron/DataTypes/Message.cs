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
        public Uri Location { get; set; }
        public Uri User { get; set; }
        public Uri SelfIdentifier { get; set; }
        public string Context { get; set; }

        [JsonProperty("message")]
        public string Contents { get; set; }
        [JsonProperty("raw")]
        public string RawContents { get; set; }

        public Message()
        {

        }
    }
}
