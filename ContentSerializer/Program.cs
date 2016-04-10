using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueLib.Tools;
using System.IO;
using Newtonsoft.Json.Linq;
using LeagueLib.Files;
using System.Threading;
using Newtonsoft.Json;
using LeagueSandbox.ContentSerializer.HashForce;

namespace LeagueSandbox.ContentSerializer
{
    public class Program
    {
        static string GetRadsPath(LaunchArguments arguments)
        {
            if (arguments.ContainsKey("radsPath")) return arguments["radsPath"];
            if (File.Exists("rads-path.txt")) return File.ReadAllText("rads-path.txt");

            throw new Exception("No RADS path defined");
        }

        static void Main(string[] args)
        {
            var arguments = LaunchArguments.Parse(args);
            var radsPath = GetRadsPath(arguments);

            var manager = new ArchiveFileManager(radsPath);
            var hashForcer = new HashForcer(true);
            hashForcer.LoadHashes(manager);
            hashForcer.LoadSources("sources.json");
            hashForcer.Run(Environment.ProcessorCount);
            hashForcer.WaitFinish();
            var result = hashForcer.GetResult();

            Console.WriteLine("Done!");
            Console.WriteLine("Found {0} sections", result.SectionCount);
            Console.WriteLine("Found {0} total hashes", result.HashCount);
            Console.WriteLine("Missing {0} hashes", hashForcer.HashCount - result.HashCount);
            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText("result.json", resultJson);
            Console.WriteLine("Saved findings to a result.json");
            Console.ReadKey();
        }
    }
}
