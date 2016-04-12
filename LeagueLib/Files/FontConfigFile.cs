using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueLib.Files
{
    public partial class FontConfigFile
    {
        public Dictionary<string, string> Content { get; private set; }
        private List<string> _header;

        private FontConfigFile()
        {
            Content = new Dictionary<string, string>();
            _header = new List<string>();
        }
    }
}
