using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        public override bool Matches(string str)
        {
            return Regex.IsMatch(str, Expression);
        }
    }
}
