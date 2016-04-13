using LeagueLib.Files;
using LeagueLib.Tools;
using LeagueSandbox.ContentSerializer.ContentTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class ItemExporter
    {
        private InibinConverter _converter;

        public ItemExporter()
        {
            _converter = new InibinConverter(ConversionMap.Load("ConversionMaps/ItemConversionMap.json"));
        }

        public void Export(ArchiveFileManager manager, FontConfigFile localization)
        {
            var files = manager.GetFileEntriesFrom("DATA/Items");
            foreach (var file in files)
            {
                // Make sure we have an inibin
                if (!file.Name.EndsWith(".inibin")) continue;

                // Load and convert
                var inibin = manager.ReadInibin(file.FullName);
                var item = Item.FromInibin(inibin, _converter, localization);

                // Find save path and create directory
                var savePath = string.Format("TestData/Items/{0}/{0}.json", item.FileName);
                var saveDirectory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                File.WriteAllText(savePath, item.Serialize());
            }
        }
    }
}
