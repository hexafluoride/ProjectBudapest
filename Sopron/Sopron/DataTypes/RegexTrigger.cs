using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.DataTypes
{
    [SopronDataType]
    public class RegexTrigger : Trigger
    {
        public string Expression { get; set; }
        public List<string> Flags { get; set; }

        public RegexTrigger()
        {

        }
    }
}
