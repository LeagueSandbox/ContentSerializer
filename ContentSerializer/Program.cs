using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using LeagueSandbox.ContentSerializer.HashForce;
using LeagueLib.Files;
using LeagueLib.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LeagueSandbox.ContentSerializer.Exporters;

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
            var manager = new ArchiveFileManager(@"C:\LOL420\RADS\projects\lol_game_client");
            NGridReader ngridReader = new NGridReader(manager.ReadFile("LEVELS/map11/AIPath.aimesh_ngrid").Uncompress());
            ngridReader.ToImage("ngrid.tga");
            ngridReader.ToNavGrid().Serialize("LEVELS/map11/AIPath.json");

            new ProgramInterfaceCLI().ConsoleInterface();
            return;
            var timer = new Stopwatch();
            timer.Start();

            var arguments = LaunchArguments.Parse(args);
            var radsPath = GetRadsPath(arguments);
            //var manager = new ArchiveFileManager(radsPath);

            timer.Stop();
            Console.WriteLine("Elapsed time: {0} ms", timer.ElapsedMilliseconds);
            Console.ReadKey();
        }

        static void TestingAndDebugging(ArchiveFileManager manager)
        {
            //ExtractSpellData(manager, "result-420-420.json");
            //ConvertDraftToMap("spellConversionMapDraft.json", "spellConversionMap.json");
            ////ReformatResult();
            //var conversionMap = ConversionMap.Load("ItemConversionMap.json");
            //var converter = new InibinConverter(conversionMap);
            //ExportData(manager, converter);
            //var mapping = (JObject)JToken.Parse(File.ReadAllText("ItemConversionMap.json"));
            //Sort(mapping);
            //File.WriteAllText("ItemConversionMapSorted.json", mapping.ToString());
            //ExtractItemData(manager, "result-420-420.json");
        }

        public static void SanitizeAndSort(JObject jObj)
        {
            var props = jObj.Properties().ToList();
            jObj.RemoveAll();

            foreach (var prop in props.OrderBy(p => p.Name))
            {
                jObj.Add(prop);
                if (prop.Value is JObject)
                {
                    SanitizeAndSort((JObject)prop.Value);
                    continue;
                }
                SanitizePropertyValue(prop);
            }
        }

        public static void SanitizePropertyValue(JProperty property)
        {
            SanitizeBooleanProperty(property);
            SanitizeDecimalProperty(property);
        }

        public static void SanitizeBooleanProperty(JProperty property)
        {
            var propVal = ((string)property.Value).ToLowerInvariant();
            switch(propVal)
            {
                case "yes":
                    property.Value = "true";
                    return;
                case "no":
                    property.Value = "false";
                    return;
            }
        }

        public static void SanitizeDecimalProperty(JProperty property)
        {
            var propVal = (string)property.Value;
            var format = "";

            if (propVal.StartsWith("."))
            {
                format = "0.";
                propVal = propVal.Remove(0, 1);
            }
            if (propVal.StartsWith("-."))
            {
                format = "-0.";
                propVal = propVal.Remove(0, 2);
            }
            if(format == "")
            {
                return;
            }

            try
            {
                var tempVal = Convert.ToDouble(propVal);
                property.Value = string.Format("{0}{1}", format, propVal);
                return;
            }
            catch { }
        }

        static void ReformatResult()
        {
            var source = "ResultFiltered.json";
            var target = "ResultFilteredReformatted.json";
            var scratchJson = File.ReadAllText(source);
            var hashCollection = JsonConvert.DeserializeObject<LeagueHashCollection>(scratchJson);
            var mapping = new Dictionary<string, Dictionary<string, uint>>();
            foreach (var entry in hashCollection.Hashes)
            {
                var hash = entry.Key;
                var section = entry.Value.First();
                var name = section.Value.First();

                if (!mapping.ContainsKey(section.Key)) mapping[section.Key] = new Dictionary<string, uint>();
                mapping[section.Key].Add(name, hash);
            }
            var mappingJson = JsonConvert.SerializeObject(mapping, Formatting.Indented);
            File.WriteAllText(target, mappingJson);
        }

        static void FilterResult()
        {
            var hashSourcesPath = "result.json";
            var data = File.ReadAllText(hashSourcesPath);
            var hashSourceCollection = JsonConvert.DeserializeObject<LeagueHashSourceCollection>(data);
            var hashCollection = new LeagueHashCollection(hashSourceCollection);

            var result = new LeagueHashCollection();
            foreach (var hash in hashCollection.Hashes)
            {
                var sectionFilterer = new SectionFilterer(hash.Value);
                sectionFilterer.ApplyFilter(new FilterPlaintextSections().SetMinimumCount<FilterPlaintextSections>(0));
                sectionFilterer.ApplyFilter(new FilterDuplicateSections().SetMinimumCount<FilterDuplicateSections>(0));
                sectionFilterer.ApplyFilter(new FilterPlaintextKeys().SetMinimumCount<FilterPlaintextKeys>(0));
                if (!(sectionFilterer.CurrentSections.Count > 0)) continue;
                result.Hashes.Add(hash.Key, sectionFilterer.CurrentSections);
            }

            var itemMappingJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText("ResultFiltered.json", itemMappingJson);
        }

        static void ExportData(ArchiveFileManager manager, InibinConverter converter)
        {
            var itemFiles = manager.GetFileEntriesFrom("DATA/Items", true);
            foreach (var entry in itemFiles)
            {
                var saveDirectory = Path.GetDirectoryName(string.Format("Content/{0}", entry.FullName));
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                var compressedFile = manager.ReadFile(entry.FullName, true);
                if (compressedFile == null) continue;
                var file = compressedFile.Uncompress();

                if (entry.FullName.Contains(".inibin") || entry.FullName.Contains(".troybin"))
                {
                    var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                    foreach (var kvp in inibin.Content)
                    {
                        converter.AddByHash(kvp.Key, kvp.Value);
                    }
                    var itemContent = converter.Deserialize();
                    var itemContentJson = JsonConvert.SerializeObject(itemContent, Formatting.Indented);

                    var savePath = string.Format("Content/{0}", entry.FullName.Replace(".inibin", ".json"));
                    if (entry.FullName.Contains(".troybin"))
                    {
                        savePath = string.Format("Content/{0}", entry.FullName.Replace(".troybin", ".troybin.json"));
                    }

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

        static void ExtractSpellData(ArchiveFileManager manager, string hashSourcesPath)
        {
            var data = File.ReadAllText(hashSourcesPath);
            var hashSourceCollection = JsonConvert.DeserializeObject<LeagueHashSourceCollection>(data);
            var hashCollection = new LeagueHashCollection(hashSourceCollection);

            var itemHashes = new HashSet<uint>();
            //From Data/Spells/NAME.inibin or Data/Shared/NAME.inibin or Data/Characters/CHARACTER/CHARACTER.inibin (no subdirectories)
            var itemFiles = manager.GetMatchFileEntries(@"Data(|\/Characters\/([^\/]+)|\/Shared)\/Spells\/([^\/]+)\.inibin");
            foreach (var entry in itemFiles)
            {
                if (!entry.FullName.Contains(".inibin")) continue;
                if (entry.FullName.Contains(".lua")) continue;
                var file = manager.ReadFile(entry.FullName).Uncompress();
                var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                foreach (var kvp in inibin.Content)
                {
                    if (itemHashes.Contains(kvp.Key)) continue;
                    itemHashes.Add(kvp.Key);
                }
            }

            var mapping = new LeagueHashCollection();
            foreach (var hash in itemHashes)
            {
                if (!hashCollection.Hashes.ContainsKey(hash)) continue;
                mapping.Hashes.Add(hash, hashCollection.Hashes[hash]);
            }

            var result = new LeagueHashCollection();
            foreach (var hash in mapping.Hashes)
            {
                var sectionFilterer = new SectionFilterer(hash.Value);
                sectionFilterer.ApplyFilter(new FilterPlaintextSections());
                sectionFilterer.ApplyFilter(new FilterDuplicateSections());
                sectionFilterer.ApplyFilter(new FilterPlaintextKeys());
                result.Hashes.Add(hash.Key, sectionFilterer.CurrentSections);
            }
            var conflictResolver = new ConflictResolver();
            //result = conflictResolver.ResolveConflicts(result);

            var itemMappingJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText("spellConversionMapDraft.json", itemMappingJson);
        }

        static void ExtractItemData(ArchiveFileManager manager, string hashSourcesPath)
        {
            var data = File.ReadAllText(hashSourcesPath);
            var hashSourceCollection = JsonConvert.DeserializeObject<LeagueHashSourceCollection>(data);
            var hashCollection = new LeagueHashCollection(hashSourceCollection);

            var itemHashes = new HashSet<uint>();
            //From Data/Items/NAME.inibin (no subdirectories)
            var itemFiles = manager.GetMatchFileEntries(@"Data\/Items\/([^\/]+)\.inibin");
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
