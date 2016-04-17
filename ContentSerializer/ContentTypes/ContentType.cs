using LeagueLib.Files;
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
    public class ContentType
    {
        public Dictionary<string, Dictionary<string, object>> Values { get; set; }
        public Dictionary<string, object> MetaData { get; set; }

        protected ContentType()
        {
            Values = new Dictionary<string, Dictionary<string, object>>();
            MetaData = new Dictionary<string, object>();
        }

        public int ContentFormatVersion
        {
            get { return (int)MetaData["ContentFormatVersion"]; }
            protected set { MetaData["ContentFormatVersion"] = value; }
        }

        public virtual object Id { get; protected set; }
        public virtual string Name { get; protected set; }
        public virtual string FileName { get; protected set; }

        protected string FilterPath(string path)
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

        protected virtual void FindId(string archiveFilePath) { }

        protected virtual void FindName(FontConfigFile localization)
        {
            Name = Id.ToString();
            FileName = Name;
            if (!Values.ContainsKey("SpellData")) return;
            if (!Values["SpellData"].ContainsKey("DisplayName")) return;

            var nameKey = (string)Values["SpellData"]["DisplayName"];
            if (string.IsNullOrEmpty(nameKey)) return;
            if (!localization.Content.ContainsKey(nameKey)) return;


            Name = localization.Content[nameKey];
            var namePossibility = FilterPath(Name);
            if (string.IsNullOrWhiteSpace(namePossibility)) return;
            FileName = namePossibility;
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

        public static T FromInibin<T>(Inibin source, InibinConverter converter, FontConfigFile localization)
            where T : ContentType, new()
        {
            var result = new T();
            result.Values = converter.Convert(source);
            result.FindId(source.FilePath);
            result.FindName(localization);
            result.ContentFormatVersion = 1;
            return result;
        }
    }
}
