using System;
using System.Collections.Generic;
using System.Text;

namespace Sopron.Messages
{
    [SopronMessage]
    public class ListModules
    {
        public ListModules()
        {

        }
    }

    [SopronMessage]
    public class ModuleList
    {
        public List<string> Modules { get; set; }

        public ModuleList()
        {

        }
    }
}
