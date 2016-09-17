using LeagueLib.Files;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace LeagueSandbox.ContentSerializer
{
    public class ContentFile
    {
        public Dictionary<string, Dictionary<string, object>> Values { get; set; }
        public Dictionary<string, object> MetaData { get; set; }

        protected ContentFile()
        {
            Values = new Dictionary<string, Dictionary<string, object>>();
            MetaData = new Dictionary<string, object>();
        }

        public int ContentFormatVersion
        {
            get { return (int)MetaData["ContentFormatVersion"]; }
            protected set { MetaData["ContentFormatVersion"] = value; }
        }

        public virtual string Name
        {
            get { return (string)MetaData["Name"]; }
            protected set { MetaData["Name"] = value; }
        }

        public virtual string ResourcePath
        {
            get { return (string)MetaData["ResourcePath"]; }
            protected set { MetaData["ResourcePath"] = value; }
        }

        public virtual object Id
        {
            get { return MetaData["Id"]; }
            protected set { MetaData["Id"] = value; }
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

        public static ContentFile FromInibin(Inibin source, ContentConfiguration configuration)
        {
            var result = new ContentFile();
            result.Values = configuration.Converter.Convert(source);
            result.Id = configuration.FindId(source.FilePath);
            result.Name = configuration.FindName(result);
            result.ResourcePath = source.FilePath;
            result.ContentFormatVersion = 4;
            return result;
        }

        public static ContentFile FromInibin(Inibin source, InibinConverter converter)
        {
            var result = new ContentFile();
            result.Values = result.Values = converter.Convert(source);
            result.Id = 0;
            result.Name = "";
            result.ResourcePath = source.FilePath;
            result.ContentFormatVersion = 2;
            return result;
        }

    }
}
