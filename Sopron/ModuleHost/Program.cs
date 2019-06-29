using Bursa;
using Sopron;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModuleHost
{
    class Program
    {
        static List<BursaModule> Modules = new List<BursaModule>();

        static async Task Main(string[] args)
        {
            SopronTypeJsonConverter.InitializeTypes();
            Console.WriteLine("ModuleHost starting...");

            if (!Directory.Exists("modules"))
                return;

            var files = Directory.GetFiles("modules", "*.dll", SearchOption.AllDirectories);

            foreach(var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                var types = assembly.GetExportedTypes();
                var module_types = types.Where(t => t.IsSubclassOf(typeof(BursaModule)));

                foreach(var type in module_types)
                {
                    var module = Activator.CreateInstance(type) as BursaModule;

                    Console.Write($"Loading {module.HumanReadableName} {module.Version} from {Path.GetFileName(file)}...");

                    module.Initialize();
                    await module.Connect(new Uri("json://127.0.0.1:3131/"));
                    await module.Handshake();

                    module.HandleMessages();

                    Modules.Add(module);

                    Console.WriteLine("success");
                }
            }

            Console.WriteLine($"Loaded {Modules.Count} modules");
            await Task.Delay(-1);
        }
    }
}
