using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IniParser.Model.Configuration;
using LeagueLib.Files;
using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class IniExporter : IExporter
    {
        public ArchiveFileManager Manager { get; set; }

        public void Export(ReleaseManifestFileEntry file, string output)
        {
            var data = Encoding.UTF8.GetString(Manager.ReadFile(file.FullName).Uncompress());
            var config = new IniParserConfiguration();
            config.CommentRegex = new Regex(@"(?:^| )(?:;|#)(.*)");
            var parser = new IniParser.Parser.IniDataParser(config);
            var ini = parser.Parse(data);
            var item = ContentFile.FromIniData(ini, file.FullName);

            var dir = Path.GetDirectoryName(output);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(output, Encoding.UTF8.GetBytes(item.Serialize().ToString()));
        }

        public void Load(ArchiveFileManager manager, FontConfigFile localization, JObject conf, string confPath)
        {
            Manager = manager;
        }
    }
}
