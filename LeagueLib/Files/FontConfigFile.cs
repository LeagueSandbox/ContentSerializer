using LeagueLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeagueLib.Files
{
    public partial class FontConfigFile
    {
        private const string FONTCONFIG_LOCATION = "DATA/Menu/fontconfig_{0}.txt";
        public Dictionary<string, string> Content { get; set; }

        public FontConfigFile(Dictionary<string, string> dict)
        {
            Content = dict;
        }

        public string Localize(string source)
        {
            if(Content.ContainsKey(source))
            {
                return Content[source];
            }
            return source;
        }

        public static FontConfigFile Load(ArchiveFileManager manager, string locale)
        {
            return Read(manager, string.Format(FONTCONFIG_LOCATION, locale));
        }
    }
}
