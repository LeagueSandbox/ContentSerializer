using LeagueLib.Files;
using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
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
    public interface IExporter
    {
        void Export(ReleaseManifestFileEntry file, string output);
        void Load(ArchiveFileManager manager, FontConfigFile localization, JObject conf, string confPath);
    }
}
