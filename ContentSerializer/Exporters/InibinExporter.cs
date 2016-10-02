using LeagueLib.Files.Manifest;
using LeagueLib.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LeagueSandbox.ContentSerializer.Exporters
{
    public class InibinExporter
    {
        public string OutputDirectory { get; }
        private ArchiveFileManager _manager;
        private ReleaseManifestFileEntry[] _files;

        public InibinExporter(ArchiveFileManager manager, string outputDirectory = "ExportOutput")
        {
            _manager = manager;
            _files = manager.GetAllFileEntries();
            OutputDirectory = outputDirectory;
        }

        public void Export(params ContentConfiguration[] list)
        {
            foreach(var configuration in list)
            {
                Export(configuration);
            }
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
            foreach(var file in _files)
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
                "{0}/{1}",
                OutputDirectory,
                configuration.GetTargetName(file.FullName)
            );

            // Create save directory if one doesn't exist
            var saveDirectory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

            // Save the converted data
            File.WriteAllText(savePath, item.Serialize());
        }
    }
}
