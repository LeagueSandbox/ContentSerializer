using LeagueLib.Files;
using LeagueSandbox.ContentSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer.ContentTypes
{
    public class Item
    {
        public Dictionary<string, Dictionary<string, object>> Values { get; set; }
        public Dictionary<string, object> MetaData { get; set; }

        public string Name
        {
            get { return (string)MetaData["ItemName"]; }
            private set { MetaData["ItemName"] = value; }
        }

        public string FileName
        {
            get { return (string)MetaData["ItemFileName"]; }
            private set { MetaData["ItemFileName"] = value; }
        }

        public int Id
        {
            get { return (int)MetaData["ItemId"]; }
            private set { MetaData["ItemId"] = value; }
        }

        public int ContentFormatVersion
        {
            get { return (int)MetaData["ContentFormatVersion"]; }
            private set { MetaData["ContentFormatVersion"] = value; }
        }

        private Item()
        {
            Values = new Dictionary<string, Dictionary<string, object>>();
            MetaData = new Dictionary<string, object>();
        }

        private void FindId(string archiveFilePath)
        {
            archiveFilePath = archiveFilePath.ToLower();
            archiveFilePath = archiveFilePath.Replace("data/items/", "");
            archiveFilePath = archiveFilePath.Replace(".inibin", "");
            if (!archiveFilePath.All(char.IsDigit)) throw new Exception("Could not extract ID from given path");
            Id = Convert.ToInt32(archiveFilePath);
        }

        private void FindName(FontConfigFile localization)
        {
            Name = Id.ToString();
            FileName = Name;
            if (!Values.ContainsKey("Data")) return;
            if (!Values["Data"].ContainsKey("DisplayName")) return;

            var nameKey = (string)Values["Data"]["DisplayName"];
            if (string.IsNullOrEmpty(nameKey)) return;
            if (!localization.Content.ContainsKey(nameKey)) return;

            Name = localization.Content[nameKey];
            var namePossibility = FilterPath(Name);
            if (string.IsNullOrWhiteSpace(namePossibility)) return;
            FileName = namePossibility;
        }

        private string FilterPath(string path)
        {
            var result = new List<char>();
            foreach (var character in path)
            {
                if (character == ' ') continue;
                if (character == ':') continue;
                if (character == '\'') continue;
                result.Add(character);
            }
            return new string(result.ToArray());
        }

        public string Serialize()
        {
            var metaData = new JProperty("MetaData", JObject.FromObject(MetaData));
            var values = new JProperty("Values", JObject.FromObject(Values));
            var data = new JObject(metaData, values);
            Program.Sort(data);
            var result = new StringWriter();
            var jsonWriter = new JsonTextWriter(result);
            var serializer = new JsonSerializer();
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = 4;
            serializer.Serialize(jsonWriter, data);
            return result.ToString();
        }

        public static Item FromInibin(Inibin source, InibinConverter converter, FontConfigFile localization)
        {
            var result = new Item();
            result.Values = converter.Convert(source);
            result.FindId(source.FilePath);
            result.FindName(localization);
            result.ContentFormatVersion = 1;
            return result;
        }
    }
}
