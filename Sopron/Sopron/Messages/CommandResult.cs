using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.Messages
{
    [SopronMessage]
    public class CommandResult
    {
        public int CallId { get; set; }
        public string Output { get; set; }

        public CommandResult()
        {

        }
    }
}
