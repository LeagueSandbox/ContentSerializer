using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueLib.Hashes
{
    public static class HashFunctions
    {
        public static uint LeagueHash(string value)
        {
            uint hash = 0;
            uint temp = 0;

            value = value.ToLower();

            for (int i = 0; i < value.Length; i++)
            {
                hash = (hash << 4) + value[i];
                temp = hash & 0xf0000000;
                if (temp != 0)
                {
                    hash = hash ^ (temp >> 24);
                    hash = hash ^ temp;
                }
            }

            return hash;
        }

        public static uint GetInibinHash(string section, string name)
        {
            uint hash = 0;
            section = section.ToLower();
            name = name.ToLower();

            foreach (var c in section)
            {
                hash = c + 65599 * hash;
            }

            hash = (65599 * hash + 42);

            foreach (var c in name)
            {
                hash = c + 65599 * hash;
            }

            return hash;
        }
    }
}
