using LeagueLib.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public static FontConfigFile Read(byte[] data)
        {
            var result = new FontConfigFile();

            var stream = new StreamReader(new MemoryStream(data));
            var line = stream.ReadLine();
            while (!line.StartsWith("tr") && !stream.EndOfStream)
            {
                result._header.Add(line);
                line = stream.ReadLine();
            }
            while (line.StartsWith("tr") && !stream.EndOfStream)
            {
                var entry = ParseLine(line);
                result.Content.Add(entry.Key, entry.Value);
                line = stream.ReadLine();
            }

            return result;
        }

        private static KeyValuePair<string, string> ParseLine(string line)
        {
            var key = GetQuotedContentAt(line, 0);
            var value = GetQuotedContentAt(line, 1);
            return new KeyValuePair<string, string>(key, value);
        }

        private static string GetQuotedContentAt(string container, int index)
        {
            index *= 2;
            var start = IndexOfAt(container, '"', index) + 1;
            var end = IndexOfAt(container, '"', index + 1);
            return container.Substring(start, end - start);
        }

        private static int IndexOfAt(string source, char character, int index)
        {
            var found = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i] != character) continue;
                if (found == index) return i;
                found++;
            }
            throw new Exception("No such index");
        }
    }
}
