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
            if (message.Length == Name.Length + 1 &&
                message.EndsWith(Name))
                return true;

            return message.Substring(1).StartsWith(Name + " ");
        }

        public override string RemoveMatch(string message)
        {
            if (message.Length == Name.Length + 1)
                return "";
            return message.Substring(Name.Length + 2);
        }
    }
}
