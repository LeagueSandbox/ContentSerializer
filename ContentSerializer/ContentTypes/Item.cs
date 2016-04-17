using LeagueLib.Files;
using LeagueSandbox.ContentSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer.ContentTypes
{
    public class Item : ContentType
    {
        public override string Name
        {
            get { return (string)MetaData["ItemName"]; }
            protected set { MetaData["ItemName"] = value; }
        }

        public override string FileName
        {
            get { return (string)MetaData["ItemFileName"]; }
            protected set { MetaData["ItemFileName"] = value; }
        }

        public override object Id
        {
            get { return MetaData["ItemId"]; }
            protected set { MetaData["ItemId"] = value; }
        }

        protected override void FindId(string archiveFilePath)
        {
            archiveFilePath = archiveFilePath.ToLower();
            archiveFilePath = archiveFilePath.Replace("data/items/", "");
            archiveFilePath = archiveFilePath.Replace(".inibin", "");
            if (!archiveFilePath.All(char.IsDigit))
                throw new Exception("Could not extract ID from given path");
            Id = Convert.ToInt32(archiveFilePath);
        }
    }
}
