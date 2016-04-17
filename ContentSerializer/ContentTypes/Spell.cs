using LeagueLib.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer.ContentTypes
{
    public class Spell : ContentType
    {
        public override string Name
        {
            get { return (string)MetaData["SpellName"]; }
            protected set { Console.WriteLine("name set to {0}", value);  MetaData["SpellName"] = value; }
        }

        public override string FileName
        {
            get { return (string)MetaData["SpellFileName"]; }
            protected set { MetaData["SpellFileName"] = value; }
        }

        public override object Id
        {
            get { return MetaData["SpellId"]; }
            protected set { MetaData["SpellId"] = value; }
        }

        protected override void FindId(string archiveFilePath)
        {
            archiveFilePath = archiveFilePath.Replace("DATA/Spells/", "");
            archiveFilePath = archiveFilePath.Replace(".inibin", "");
            Id = archiveFilePath;
        }
    }
}
