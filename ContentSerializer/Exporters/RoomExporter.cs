using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueLib.Files;
using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class RoomExporter : IExporter
    {
        public ArchiveFileManager Manager { get; set; }

        public void Export(ReleaseManifestFileEntry file, string output)
        {
            throw new NotImplementedException();
        }

        public void Load(ArchiveFileManager manager, FontConfigFile localization, JObject conf, string confPath)
        {
            Manager = manager;
            throw new NotImplementedException();
        }
    }
}
