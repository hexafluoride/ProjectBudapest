using Sopron.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.Messages
{
    [SopronMessage]
    public class CommandNotification
    {
        public string Id { get; set; }
        public Message Message { get; set; }
        public int CallId { get; set; }

        public CommandNotification()
        {

        }
    }
}
