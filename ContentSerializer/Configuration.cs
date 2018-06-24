using LeagueLib.Files;
using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
using LeagueSandbox.ContentSerializer.Exporters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer
{
    public class Configuration
    {
        // json structure
        private class ConfigurationConf
        {
            public string InputPattern { get; set; }
            public string OutputPattern { get; set; }
            public string ExporterType { get; set; }
            public JObject ExporterConf { get; set; }
        }

        public Regex InputPattern { get; set; }
        public string OutputPattern { get; set;  }
        public IExporter Exporter { get; set; }

        public Configuration(ArchiveFileManager manager, FontConfigFile localizatio, Regex inputPattern, 
            string outputPattern, IExporter exporter)
        {
            InputPattern = inputPattern;
            OutputPattern = outputPattern;
            Exporter = exporter;
        }

        private Configuration(ArchiveFileManager manager, FontConfigFile localization,
            ConfigurationConf conf, string confPath)
        {
            InputPattern = new Regex(conf.InputPattern, RegexOptions.IgnoreCase);
            OutputPattern = conf.OutputPattern;
            var ExporterType = Type.GetType($"LeagueSandbox.ContentSerializer.Exporters.{conf.ExporterType}");
            Exporter = (IExporter)(ExporterType.GetConstructor(new Type[] {}).Invoke(new object[] { }));
            Exporter.Load( manager, localization, conf.ExporterConf, confPath);
        }

        public bool Export(ReleaseManifestFileEntry file, string output)
        {
            if(InputPattern.IsMatch(file.FullName))
            {
                Exporter.Export(file, $"{output}/{InputPattern.Replace(file.FullName, OutputPattern)}");
                return true;
            }
            return false;
        }

        public static Configuration[] Load(ArchiveFileManager manager, FontConfigFile localization,  string confPath)
        {
            var list = JsonConvert.DeserializeObject<ConfigurationConf[]>(File.ReadAllText(confPath));
            confPath = Path.GetDirectoryName(confPath);
            return list.Select(c => new Configuration(manager, localization, c, confPath)).ToArray();
        }
    }
}
