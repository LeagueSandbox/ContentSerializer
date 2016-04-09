using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueLib.Tools;
using System.IO;

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
            var fileEntries = manager.GetAllFileEntries().OrderBy(x => x.FullName);

            foreach (var entry in fileEntries)
            {
                Console.WriteLine(entry.FullName);
            }
        }
    }
}
