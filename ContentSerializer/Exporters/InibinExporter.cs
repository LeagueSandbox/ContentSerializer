using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class InibinExporter
    {
        private ArchiveFileManager _manager;

        public InibinExporter(ArchiveFileManager manager)
        {
            _manager = manager;
        }

        public void Export(ContentConfiguration configuration)
        {
            // Has to be done this way so we can use the patterns for priority in case of conflicts
            foreach (var pattern in configuration.ResourcePatterns)
            {
                ExportMatches(configuration, pattern);
            }
        }

        private void ExportMatches(ContentConfiguration configuration, Regex pattern)
        {
            var files = _manager.GetAllFileEntries();
            foreach(var file in files)
            {
                if (!pattern.IsMatch(file.FullName)) continue;
                ExportFile(configuration, file);
            }
        }

        private void ExportFile(ContentConfiguration configuration, ReleaseManifestFileEntry file)
        {
            // Load and convert
            var inibin = _manager.ReadInibin(file.FullName);
            var item = ContentFile.FromInibin(inibin, configuration);

            // Find save path
            var savePath = string.Format(
                "ExportOutput/{0}/{1}/{1}.json",
                configuration.OutputDirectory,
                item.Id.ToString()
            );

            // Create save directory if one doesn't exist
            var saveDirectory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

            // Save the converted data
            File.WriteAllText(savePath, item.Serialize());
        }
    }
}
