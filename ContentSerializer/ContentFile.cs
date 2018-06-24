using IniParser.Model;
using LeagueLib.Files;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeagueSandbox.ContentSerializer
{
    public class ContentFile
    {
        public Dictionary<string, Dictionary<string, object>> Values { get; set; }
        public Dictionary<string, object> MetaData { get; set; }

        public ContentFile()
        {
            Values = new Dictionary<string, Dictionary<string, object>>();
            MetaData = new Dictionary<string, object>();
        }

        public int ContentFormatVersion
        {
            get { return (int)MetaData["ContentFormatVersion"]; }
            set { MetaData["ContentFormatVersion"] = value; }
        }

        public virtual string Name
        {
            get { return (string)MetaData["Name"]; }
            set { MetaData["Name"] = value; }
        }

        public virtual string ResourcePath
        {
            get { return (string)MetaData["ResourcePath"]; }
            set { MetaData["ResourcePath"] = value; }
        }

        public virtual object Id
        {
            get { return MetaData["Id"]; }
            set { MetaData["Id"] = value; }
        }

        public string Serialize()
        {
            var metaData = new JProperty("MetaData", JObject.FromObject(MetaData));
            var values = new JProperty("Values", JObject.FromObject(Values));
            var data = new JObject(metaData, values);
            Program.SanitizeAndSort(data);
            var result = new StringWriter();
            var jsonWriter = new JsonTextWriter(result);
            var serializer = new JsonSerializer();
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = 4;
            
            serializer.Serialize(jsonWriter, data);
            return result.ToString();
        }

        public static ContentFile FromIniData(IniData source, string filePath)
        {
            var result = new ContentFile();
            foreach(var section in source.Sections)
            {
                var dict = new Dictionary<string, object>();
                foreach(var name in section.Keys)
                {
                    dict[name.KeyName] = name.Value;
                }
                result.Values[section.SectionName] = dict;
            }
            {
                var dict = new Dictionary<string, object>();
                foreach (var global in source.Global)
                {
                    dict[global.KeyName] = global.Value;
                }
                result.Values[""] = dict;
            }
            result.Id = filePath.Split('/').Last().Split('.').First();
            result.Name = result.Id.ToString();
            result.ResourcePath = filePath;
            result.ContentFormatVersion = 4;
            return result;
        }

        public static ContentFile FromInibin(Inibin source, InibinConverter converter)
        {
            var result = new ContentFile();
            result.Values = converter.Convert(source);
            result.Id = source.FilePath.Split('/').Last().Split('.').First();
            result.Name = result.Id.ToString();
            result.ResourcePath = source.FilePath;
            result.ContentFormatVersion = 4;
            return result;
        }
    }
}
