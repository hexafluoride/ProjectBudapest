using Sopron.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.Messages
{
    [SopronMessage]
    public class RegisterCommand
    {
        public Command Command { get; set; }
        public RegisterCommand()
        {

        }
    }
}
