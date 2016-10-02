using LeagueLib.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace LeagueSandbox.ContentSerializer
{
    public class ContentConfiguration
    {
        public Regex InputPattern { get; }
        public string OutputPattern { get; }
        public List<Regex> ResourcePatterns { get; }
        public FontConfigFile Localization { get; }
        public InibinConverter Converter { get; }

        private static readonly List<Tuple<string, string>> NameFields = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("Data", "DisplayName")
        };

        public ContentConfiguration(FontConfigFile localization, InibinConverter converter,
            List<Regex> patterns, Regex inputPattern, string outputPattern)
        {
            Localization = localization;
            Converter = converter;
            ResourcePatterns = patterns;
            InputPattern = inputPattern;
            OutputPattern = outputPattern;
        }

        public string GetTargetName(string archiveFilePath)
        {
            return InputPattern.Replace(archiveFilePath, OutputPattern);
        }

        public object FindId(string archiveFilePath)
        {
            return archiveFilePath.Split('/').Last().Replace(".inibin", "");
        }

        public string FindName(ContentFile content)
        {
            var name = content.Id.ToString();
            
            foreach(var field in NameFields)
            {
                if (!content.Values.ContainsKey(field.Item1))
                    continue;
                if (!content.Values[field.Item1].ContainsKey(field.Item2))
                    continue;
                var nameKey = content.Values[field.Item1][field.Item2].ToString();
                if (string.IsNullOrEmpty(nameKey))
                    continue;
                if (!Localization.Content.ContainsKey(nameKey))
                    continue;
                return Localization.Content[nameKey];
            }
            return name;
        }

        public static List<Regex> LoadPatterns(string file)
        {
            var data = File.ReadAllText(file);
            var patterns = JsonConvert.DeserializeObject<List<string>>(data);
            var patternsRegex = new List<Regex>();
            foreach (var pattern in patterns)
            {
                patternsRegex.Add(new Regex(pattern, RegexOptions.IgnoreCase));
            }
            return patternsRegex;
        }

        public static ContentConfiguration Load(FontConfigFile localization, string patternsFile,
            string mapFile, string InputPattern, string outputPattern)
        {
            var converter = new InibinConverter(ConversionMap.Load(mapFile));
            var patterns = LoadPatterns(patternsFile);
            return new ContentConfiguration(localization, converter, patterns,
                new Regex(InputPattern, RegexOptions.IgnoreCase), outputPattern);
        }

        public static ContentConfiguration[] LoadList(FontConfigFile localization,
            string mapDir, string patternDir, string listFile)
        {
            var list = new List<ContentConfiguration>();
            var data = File.ReadAllText(listFile);
            var entryList = JsonConvert.DeserializeObject<List<ContentConfigurationListEntry>>(data);
            foreach(var entry in entryList)
            {
                list.Add(ContentConfiguration.Load(localization, 
                    string.Format("{0}/{1}", patternDir, entry.Pattern),
                    string.Format("{0}/{1}", mapDir, entry.ConversionMap), 
                    entry.InputPattern,
                    entry.OutputPattern));
            }
            return list.ToArray();
        }
    }

    class ContentConfigurationListEntry
    {
        public string ConversionMap { get; set; }
        public string Pattern { get; set; }
        public string InputPattern { get; set; }
        public string OutputPattern { get; set; }
    }
}
