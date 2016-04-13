using LeagueLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueLib.Files
{
    public partial class FontConfigFile
    {
        private const string FONTCONFIG_LOCATION = "DATA/Menu/fontconfig_{0}.txt";

        public Dictionary<string, string> Content { get; private set; }
        private List<string> _header;

        private FontConfigFile()
        {
            _header = new List<string>();
            Content = new Dictionary<string, string>();
        }

        public static FontConfigFile Load(ArchiveFileManager manager, string locale)
        {
            return Read(manager, string.Format(FONTCONFIG_LOCATION, locale));
        }
    }
}
