using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.DataTypes
{
    [SopronDataType]
    public abstract class Trigger
    {
        public abstract bool Matches(string message);
    }
}
