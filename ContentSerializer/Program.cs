using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using LeagueSandbox.ContentSerializer.HashForce;
using LeagueLib.Files;
using LeagueLib.Tools;
using Newtonsoft.Json;

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

            timer.Stop();
            Console.WriteLine("Elapsed time: {0} ms", timer.ElapsedMilliseconds);
            Console.ReadKey();
        }

        static void TestingAndDebugging(ArchiveFileManager manager)
        {

            //ExtractItemData(manager);
            //ConvertDraftToMap("itemConversionMapDraft.json", "itemConversionMap.json");

            var conversionMap = ConversionMap.Load("itemConversionMap.json");
            var converter = new InibinConverter(conversionMap);
            ExportItemData(manager, converter);
            //ExtractItemData(manager, "result-420-420.json");
        }

        static void ExportItemData(ArchiveFileManager manager, InibinConverter converter)
        {
            var itemFiles = manager.GetAllFileEntries();
            foreach (var entry in itemFiles)
            {
                var saveDirectory = Path.GetDirectoryName(string.Format("Content/{0}", entry.FullName));
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                var compressedFile = manager.ReadFile(entry.FullName, true);
                if (compressedFile == null) continue;
                var file = compressedFile.Uncompress();

                if (entry.FullName.Contains(".inibin"))
                {
                    var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                    foreach (var kvp in inibin.Content)
                    {
                        converter.AddByHash(kvp.Key, kvp.Value);
                    }
                    var itemContent = converter.Deserialize();
                    var itemContentJson = JsonConvert.SerializeObject(itemContent, Formatting.Indented);
                    var savePath = string.Format("Content/{0}", entry.FullName.Replace(".inibin", ".json"));
                    File.WriteAllText(savePath, itemContentJson);
                    converter.Clear();
                }
                else
                {
                    var savePath = string.Format("Content/{0}", entry.FullName);
                    File.WriteAllBytes(savePath, file);
                }
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

        static void ExtractItemData(ArchiveFileManager manager, string hashSourcesPath)
        {
            var data = File.ReadAllText(hashSourcesPath);
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

            var result = new LeagueHashCollection();
            foreach(var hash in mapping.Hashes)
            {
                var sectionFilterer = new SectionFilterer(hash.Value);
                sectionFilterer.ApplyFilter(new FilterPlaintextSections());
                sectionFilterer.ApplyFilter(new FilterDuplicateSections());
                sectionFilterer.ApplyFilter(new FilterPlaintextKeys());
                result.Hashes.Add(hash.Key, sectionFilterer.CurrentSections);
            }
            var conflictResolver = new ConflictResolver();
            result = conflictResolver.ResolveConflicts(result);

            var itemMappingJson = JsonConvert.SerializeObject(result, Formatting.Indented);
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
