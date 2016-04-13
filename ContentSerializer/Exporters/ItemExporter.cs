using LeagueLib.Files;
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
    public class ItemExporter
    {
        public const string CONVERSIONMAP_PATH = "ConversionMaps/ItemConversionMap.json";
        public const string ITEM_NAME_KEY = "DisplayName";
        public const string ITEM_DATA_PATH = "DATA/Items";

        private InibinConverter _converter;

        public ItemExporter()
        {
            _converter = new InibinConverter(ConversionMap.Load(CONVERSIONMAP_PATH));
        }

        public void Export(ArchiveFileManager manager, FontConfigFile nameInfo)
        {
            var files = manager.GetFileEntriesFrom(ITEM_DATA_PATH);
            foreach (var file in files)
            {
                // Make sure we have an inibin
                if (!file.Name.EndsWith(".inibin")) continue;

                // Load and convert
                var inibin = manager.ReadInibin(file.FullName);
                var converted = _converter.Convert(inibin);

                // Find the name from the localization map if it exists
                var name = file.Name.Remove(file.Name.Length - 7, 7);
                if(converted.ContainsKey("Data") && converted["Data"].ContainsKey(ITEM_NAME_KEY))
                {
                    var nameKey = (string)converted["Data"][ITEM_NAME_KEY];
                    if (!string.IsNullOrEmpty(nameKey) && nameInfo.Content.ContainsKey(nameKey))
                    {
                        name = nameInfo.Content[nameKey];
                    }
                }
                name = FilterPath(name);

                // Find save path and create directory
                var savePath = string.Format("TestData/Items/{0}/{0}.json", name);
                var saveDirectory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                var saveData = JObject.FromObject(converted);
                Program.Sort(saveData);
                File.WriteAllText(savePath, saveData.ToString());
            }
        }

        private string FilterPath(string path)
        {
            var result = new List<char>();
            foreach(var character in path)
            {
                if (character == ' ') continue;
                if (character == ':') continue;
                if (character == '\'') continue;
                result.Add(character);
            }
            return new string(result.ToArray());
        }
    }
}
