using LeagueLib.Files;
using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class PreloadExporter : IExporter
    {
        public ArchiveFileManager Manager { get; set; }
        private static readonly Regex RE_PRELOAD
            = new Regex("\\{Function=BBPreload(Character|Spell|Particle), Params=\\{Name=\"(.+)\"\\}\\},");

        public void Export(ReleaseManifestFileEntry file, string output)
        {
            var data = Encoding.UTF8.GetString(Manager.ReadFile(file.FullName).Uncompress());
            var dump = new Dictionary<string, List<string>>()
            {
                { "Character", new List<string>{ } },
                { "Particle", new List<string>{ } },
                { "Spell", new List<string>{ } },
            };
            foreach(Match match in RE_PRELOAD.Matches(data))
            {
                dump[match.Groups[1].Value].Add(match.Groups[2].Value);
            }
            var json = JObject.FromObject(dump);
            //Program.SanitizeAndSort(json);
            var result = new StringWriter();
            var jsonWriter = new JsonTextWriter(result);
            var serializer = new JsonSerializer();
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = 4;
            serializer.Serialize(jsonWriter, json);
            var dir = Path.GetDirectoryName(output);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(output, Encoding.UTF8.GetBytes(result.ToString()));
        }

        public void Load(ArchiveFileManager manager, FontConfigFile localization, JObject conf, string confPath)
        {
            Manager = manager;
        }
    }
}
