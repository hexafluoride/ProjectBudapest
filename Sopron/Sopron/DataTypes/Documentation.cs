using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.DataTypes
{
    [SopronDataType]
    public class Documentation
    {
        public List<string> Synopsis { get; set; }
        public string Brief { get; set; }
        public string Complete { get; set; }
        public List<string> SeeAlso { get; set; }

        public Documentation()
        {

        }
    }
}
