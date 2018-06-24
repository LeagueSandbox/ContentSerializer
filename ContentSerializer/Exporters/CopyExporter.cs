using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueLib.Files;
using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class CopyExporter : IExporter
    {
        public ArchiveFileManager Manager { get; set; }

        public void Export(ReleaseManifestFileEntry file, string output)
        {
            var dir = Path.GetDirectoryName(output);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(output, Manager.ReadFile(file.FullName).Uncompress());
        }

        public void Load(ArchiveFileManager manager, FontConfigFile localization, JObject conf, string confPath)
        {
            Manager = manager;
        }
    }
}
