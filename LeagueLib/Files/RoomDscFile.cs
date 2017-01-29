using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueLib.Files
{
    public class RoomDscEntry
    {
        public string Name { get; set; }
        public int Number { get; set; }
    }

    public class RoomDscFile
    {
        public List<RoomDscEntry> entries { get; set; } = new List<RoomDscEntry>();

        public RoomDscFile(byte[] data) : this(new StreamReader(new MemoryStream(data)))
        {
        }

        public RoomDscFile(StreamReader sr)
        {
            while(!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var split = line.Split(new char[] { ' ' });
                if (split.Count() == 2)
                {
                    var entry = new RoomDscEntry();
                    entry.Name = split[0];
                    if(entry.Name.Count() > 4)
                    {
                        entry.Name = entry.Name.Substring(0, entry.Name.Count() - 4);
                    }
                    entry.Number = int.Parse(split[1]);
                    entries.Add(entry);
                }
            }
        }
    }
}
