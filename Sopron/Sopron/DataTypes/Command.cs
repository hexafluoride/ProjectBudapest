using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.DataTypes
{
    [SopronDataType]
    public class Command
    {
        public string Id { get; set; }
        public List<string> Contexts { get; set; }
        public List<Trigger> Triggers { get; set; }
        public Documentation Documentation { get; set; }

        public Command()
        {

        }
    }
}
