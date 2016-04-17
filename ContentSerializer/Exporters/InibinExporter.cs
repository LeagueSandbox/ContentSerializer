using LeagueLib.Files;
using LeagueLib.Tools;
using LeagueSandbox.ContentSerializer.ContentTypes;
using System.IO;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class InibinExporter<T> where T : ContentType, new()
    {
        private InibinConverter _converter;
        private string _inputDirectory;
        private string _outputDirectory;

        public InibinExporter(string conversionMap, string inputDirectory, string outputDirectory)
        {
            _inputDirectory = inputDirectory;
            _outputDirectory = outputDirectory;
            _converter = new InibinConverter(ConversionMap.Load(conversionMap));
        }

        public void Export(ArchiveFileManager manager, FontConfigFile localization)
        {
            var files = manager.GetFileEntriesFrom(_inputDirectory);
            foreach (var file in files)
            {
                // Make sure we have an inibin
                if (!file.Name.EndsWith(".inibin")) continue;
                if (file.Name.Contains(".lua")) continue;

                // Load and convert
                var inibin = manager.ReadInibin(file.FullName);
                var item = ContentType.FromInibin<T>(inibin, _converter, localization);

                // Find save path and create directory
                var savePath = string.Format("{0}{1}/{1}.json", _outputDirectory, item.FileName);
                var saveDirectory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                File.WriteAllText(savePath, item.Serialize());
            }
        }
    }
}
