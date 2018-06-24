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
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class LuaobjExporter : IExporter
    {
        public ArchiveFileManager Manager { get; set; }
        public LuaDumper Dumper { get; set; }

        public void Export(ReleaseManifestFileEntry file, string output)
        {
            var data = Manager.ReadFile(file.FullName).Uncompress();
            var dump = Dumper.LoadData(data);

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
            Dumper = new LuaDumper();
        }
    }
}
