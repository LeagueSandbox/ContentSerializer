using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueLib.Tools;
using System.IO;
using Newtonsoft.Json.Linq;
using LeagueLib.Files;
using System.Threading;
using Newtonsoft.Json;
using LeagueSandbox.ContentSerializer.HashForce;
using System.Diagnostics;

namespace LeagueSandbox.ContentSerializer
{
    public class Program
    {
        static string GetRadsPath(LaunchArguments arguments)
        {
            if (arguments.ContainsKey("radsPath")) return arguments["radsPath"];
            if (File.Exists("rads-path.txt")) return File.ReadAllText("rads-path.txt");

            throw new Exception("No RADS path defined");
        }

        static void Main(string[] args)
        {
            var timer = new Stopwatch();
            timer.Start();

            var arguments = LaunchArguments.Parse(args);
            var radsPath = GetRadsPath(arguments);
            var manager = new ArchiveFileManager(radsPath);

            //ExtractItemData(manager);
            //ConvertDraftToMap("itemConversionMapScratch.json", "itemConversionMap.json");

            //var conversionMap = ConversionMap.Load("itemConversionMap.json");
            //var converter = new InibinConverter(conversionMap);
            //ExportItemData(manager, converter);
            MatchHashes(manager, "sources-420.json");
            timer.Stop();
            Console.WriteLine("Elapsed time: {0} ms", timer.ElapsedMilliseconds);
            Console.ReadKey();
        }

        static void ExportItemData(ArchiveFileManager manager, InibinConverter converter)
        {
            var itemFiles = manager.GetAllFileEntries("DATA/Items");
            foreach (var entry in itemFiles)
            {
                if (!entry.FullName.Contains(".inibin")) continue;
                var file = manager.ReadFile(entry.FullName).Uncompress();
                var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                foreach (var kvp in inibin.Content)
                {
                    converter.AddByHash(kvp.Key, kvp.Value);
                }
                var itemContent = converter.Deserialize();
                var itemContentJson = JsonConvert.SerializeObject(itemContent, Formatting.Indented);
                var savePath = string.Format("Content/{0}", entry.FullName.Replace(".inibin", ".json"));
                var saveDirectory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                File.WriteAllText(savePath, itemContentJson);
                converter.Clear();
            }
        }

        static void ConvertDraftToMap(string source, string target)
        {
            var scratchJson = File.ReadAllText(source);
            var hashCollection = JsonConvert.DeserializeObject<LeagueHashCollection>(scratchJson);
            var mapping = new Dictionary<string, Dictionary<string, uint>>();
            foreach(var entry in hashCollection.Hashes)
            {
                var hash = entry.Key;
                if (entry.Value.Count != 1) throw new Exception("Invalid conversion map draft");
                var section = entry.Value.First();
                if (section.Value.Count != 1) throw new Exception("Invalid conversion map draft");
                var name = section.Value.First();

                if (!mapping.ContainsKey(section.Key)) mapping[section.Key] = new Dictionary<string, uint>();
                mapping[section.Key].Add(name, hash);
            }
            var mappingJson = JsonConvert.SerializeObject(mapping, Formatting.Indented);
            File.WriteAllText(target, mappingJson);
        }

        static void ExtractItemData(ArchiveFileManager manager)
        {
            var data = File.ReadAllText("result-latest-420.json");
            var hashSourceCollection = JsonConvert.DeserializeObject<LeagueHashSourceCollection>(data);
            var hashCollection = new LeagueHashCollection();
            foreach (var kvp in hashSourceCollection.Content)
            {
                foreach (var name in kvp.Value)
                {
                    hashCollection.AddFromSource(kvp.Key, name);
                }
            }

            var itemHashes = new HashSet<uint>();
            var itemFiles = manager.GetAllFileEntries("DATA/Items");
            foreach(var entry in itemFiles)
            {
                if (!entry.FullName.Contains(".inibin")) continue;
                var file = manager.ReadFile(entry.FullName).Uncompress();
                var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                foreach(var kvp in inibin.Content)
                {
                    if (itemHashes.Contains(kvp.Key)) continue;
                    itemHashes.Add(kvp.Key);
                }
            }

            var mapping = new LeagueHashCollection();
            foreach(var hash in itemHashes)
            {
                if (!hashCollection.Hashes.ContainsKey(hash)) continue;
                mapping.Hashes.Add(hash, hashCollection.Hashes[hash]);
            }

            foreach(var hash in mapping.Hashes)
            {
                var sections = hash.Value;
                if (sections.ContainsKey("data") && sections.ContainsKey("Data") && sections.ContainsKey("DATA"))
                {
                    var data1 = sections["data"];
                    var data2 = sections["Data"];
                    var data3 = sections["DATA"];
                    if (!(data1.Count == 1 && data2.Count == 1 && data3.Count == 1)) continue;
                    if (!(data1.Contains(data2.First()) && data2.Contains(data3.First()))) continue;
                    sections.Remove("data");
                    sections.Remove("DATA");
                }
            }

            var itemMappingJson = JsonConvert.SerializeObject(mapping, Formatting.Indented);
            File.WriteAllText("itemConversionMapDraft.json", itemMappingJson);
        }

        static void MatchHashes(ArchiveFileManager manager, string sourcesPath)
        {
            var hashForcer = new HashForcer(true);
            hashForcer.LoadHashes(manager);
            hashForcer.LoadSources(sourcesPath);
            hashForcer.Run(Environment.ProcessorCount);
            hashForcer.WaitFinish();
            var result = hashForcer.GetResult();

            Console.WriteLine("Done!");
            Console.WriteLine("Found {0} sections", result.SectionCount);
            Console.WriteLine("Found {0} total hashes", result.HashCount);
            Console.WriteLine("Missing {0} hashes", hashForcer.HashCount - result.HashCount);
            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText("result.json", resultJson);
            Console.WriteLine("Saved findings to a result.json");
        }
    }
}
