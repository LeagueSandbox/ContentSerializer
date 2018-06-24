using LeagueLib.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeagueLib.Files
{
    public partial class FontConfigFile
    {

        public static FontConfigFile Read(ArchiveFileManager manager, string archiveFilePath)
        {
            return Read(manager.ReadFile(archiveFilePath).Uncompress());
        }

        public static FontConfigFile Read(string filePath)
        {
            return Read(File.ReadAllBytes(filePath));
        }

        private static readonly Regex RE_TR = new Regex(@"tr\s*""(.*)""\s*=\s*""(.*)""");
        public static FontConfigFile Read(byte[] data)
        {
            var dict = new Dictionary<string, string>();
            string realData = Encoding.UTF8.GetString(data);
            foreach(Match match in RE_TR.Matches(realData))
            {
                dict[match.Groups[1].Value] = match.Groups[2].Value;
            }
            return new FontConfigFile(dict);
        }
    }
}
