using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LeagueLib.Files;
using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class InibinExporterConf
    {
        public string ConversionMap { get; set; }
        public List<string[]> NameFields { get; set; } = new List<string[]>
        {
            new string[] { "Data", "DisplayName" },
            new string[] { "SpellData", "DisplayName" },
            new string[] { "Data", "Name" },
            new string[] { "SpellData", "Name" },
        };
    }

    public class InibinExporter : IExporter
    {
        public ArchiveFileManager Manager { get; set; }
        public ConversionMap ConversionMap { get; set; }
        public FontConfigFile Localization { get; set; }
        public List<string[]> NameFields { get; set; } = new List<string[]>
        {
            new string[] { "Data", "DisplayName" },
            new string[] { "SpellData", "DisplayName" },
            new string[] { "Data", "Name" },
            new string[] { "SpellData", "Name" },
        };

        public void Load(ArchiveFileManager manager, FontConfigFile localization, InibinExporterConf conf, string confPath)
        {
            if(string.IsNullOrEmpty(conf.ConversionMap))
            {
                throw new KeyNotFoundException("Missing or empty ConversionMap path ");
            }
            Manager = manager;
            Localization = localization;
            NameFields = conf.NameFields;
            ConversionMap = ConversionMap.Load($"{confPath}/{conf.ConversionMap}");
        }

        public void Load(ArchiveFileManager manager, FontConfigFile localization, JObject conf, string confPath)
        {
            Load(manager, localization, conf.ToObject<InibinExporterConf>(), confPath);
        }

        public string FindName(ContentFile content)
        {
            var name = content.Id.ToString();

            foreach (var field in NameFields)
            {
                var value = content.GetValue(field[0], field[1], Localization);
                if(value != null)
                {
                    return value;
                }
            }
            return name;
        }

        public void Export(ReleaseManifestFileEntry file, string output)
        {
            var inibin = Manager.ReadInibin(file.FullName);
            var item = ContentFile.FromInibin(inibin, ConversionMap);
            item.Name = FindName(item);
            var dir = Path.GetDirectoryName(output);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(output, Encoding.UTF8.GetBytes(item.Serialize(Localization).ToString()));
        }
    }
}
