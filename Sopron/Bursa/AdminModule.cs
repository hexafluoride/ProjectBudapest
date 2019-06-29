using Sopron.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bursa
{
    public class AdminModule : BursaModule
    {
        public override string Name => "admin";
        public override string HumanReadableName => "Admin Module";
        public override string Version => "1.0";
        public override string License => "MIT"; 

        [BursaCommand("module list")]
        public async Task<string> ListModules(object sender, CommandHandlerEventArgs e)
        {
            Console.WriteLine("We're getting there...");
            var modules = await SendWait<ModuleList>(new ListModules());

            return $"{modules.Modules.Count} module(s) loaded: [{string.Join(", ", modules.Modules)}]";
        }

        [BursaCommand("module info")]
        public async Task<string> GetModuleInfo(object sender, CommandHandlerEventArgs e)
        {
            var module_name = e.Arguments;
            var module = await SendWait<ModuleInfo>(new GetModuleInfo(module_name));

            return $"{module.ClientHello.HumanReadableName} " +
                $"({string.Join(", ", new[] { module.ClientHello.Version, module.ClientHello.Language, module.ClientHello.License }.Where(t => !string.IsNullOrEmpty(t)))}) " +
                $"has been up for {Utilities.TimeSpanToPrettyString(module.Uptime)} and is {module.Health.ToString().ToLower()}";
        }
    }
}
