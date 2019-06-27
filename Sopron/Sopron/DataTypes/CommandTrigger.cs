using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.DataTypes
{
    [SopronDataType]
    public class CommandTrigger : Trigger
    {
        public string Name { get; set; }
        public CommandTrigger()
        {

        }

        public override bool Matches(string message)
        {
            return message.Substring(1).StartsWith(Name);
        }
    }
}
