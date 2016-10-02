using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using LeagueSandbox.ContentSerializer.HashForce;
using LeagueLib.Files;
using LeagueLib.Tools;
using LeagueLib.Hashes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using LeagueLib.Files.Manifest;
using System.Text.RegularExpressions;
using LeagueSandbox.ContentSerializer.Exporters;

namespace LeagueSandbox.ContentSerializer
{
    class ProgramInterface
    {
        private ArchiveFileManager _manager;
        private List<ReleaseManifestFileEntry> _files;
        private LeagueHashCollection _draft;
        private List<string> _strings;
        private HashSet<uint> _hashes;
        private List<string> _paterns;

        
        public ProgramInterface()
        {
            _hashes = new HashSet<uint>();
            _manager = null;
            _files = new List<ReleaseManifestFileEntry>();
            _draft = new LeagueHashCollection();
            _strings = new List<string>();
            _paterns = new List<string>();
        }


        public void LoadManager(string path)
        {
            _manager = new ArchiveFileManager(path);
        }

        public void AddPatern(string patern)
        {
            if (!_paterns.Contains(patern))
            {
                _paterns.Add(patern);
            }
            var files = _manager.GetAllFileEntries();
            foreach (var entry in files)
            {
                if (new Regex(patern, RegexOptions.IgnoreCase).Match(entry.FullName).Success)
                {
                    _files.Add(entry);
                    var file = _manager.ReadFile(entry.FullName).Uncompress();
                    var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                    foreach (var kvp in inibin.Content)
                    {
                        if (!_hashes.Contains(kvp.Key))
                        {
                            _hashes.Add(kvp.Key);
                        }
                    }
                }
            }
        }
        
        public void ListPaterns()
        {
            int c = 0;
            foreach(var patern in _paterns)
            {
                Console.WriteLine(c + " - " + patern);
                c++;
            }
        }

        public void RemovePatern(int index)
        {
            if (_paterns.Count> index && index > -1)
            {

            }
            string patern = _paterns[index];
            _paterns.Remove(patern);
            _files.Clear();
            _hashes.Clear();
            foreach(string pat in _paterns)
            {
                AddPatern(pat);
            }

        }

        public void ClearFiles()
        {
            _paterns.Clear();
            _files.Clear();
            _hashes.Clear();
        }

        public void HashStrings()
        {
            var hashForcer = new HashForcer(true);
            hashForcer.SetHashes(_hashes);
            hashForcer.SetSources(_strings);
            hashForcer.Run(Environment.ProcessorCount);
            hashForcer.WaitFinish();
            var mapping = new LeagueHashCollection(hashForcer.GetResult());

            var result = new LeagueHashCollection();
            foreach (var hash in mapping.Hashes)
            {
                var sectionFilterer = new SectionFilterer(hash.Value);
                sectionFilterer.ApplyFilter(new FilterPlaintextSections());
                sectionFilterer.ApplyFilter(new FilterDuplicateSections());
                sectionFilterer.ApplyFilter(new FilterPlaintextKeys());
                result.Hashes.Add(hash.Key, sectionFilterer.CurrentSections);
            }
            _draft = result;
        }

        public int ImportStrings(string path)
        {
            if (!File.Exists(path))
                return 1;
            var list = JArray.Parse(File.ReadAllText(path));
            foreach (string item in list)
            {
                if (!_strings.Contains(item, StringComparer.OrdinalIgnoreCase))
                {
                    _strings.Add(item);
                }
            }
            return 0;
        }
        
        public int ImportDraftStrings(string input)
        {
            if (!File.Exists(input))
                return 1;
            var data = File.ReadAllText(input);
            var draft = JsonConvert.DeserializeObject<LeagueHashCollection>(data);
            ImportDraftStrings(draft);
            return 0;
        }

        public void ImportDraftStrings(LeagueHashCollection col)
        {
            foreach (var hash in col.Hashes)
            {
                foreach (var section in hash.Value)
                {
                    if (!_strings.Contains(section.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        _strings.Add(section.Key);
                    }
                    foreach (var name in section.Value)
                    {

                        if (!_strings.Contains(name, StringComparer.OrdinalIgnoreCase))
                        {
                            _strings.Add(name);
                        }
                    }
                }
            }
        }

        public void SyncStrings()
        {
            ImportDraftStrings(_draft);
        }

        public void ClearStrings()
        {
            _strings.Clear();
        }



        public void ExportDraft(string output)
        {
            var data = JsonConvert.SerializeObject(_draft, Formatting.Indented);
            File.WriteAllText(output, data);
        }

        public void ExportStrings(string output)
        {
            File.WriteAllText(output, JsonConvert.SerializeObject(_strings, Formatting.Indented));
        }

        public void ExportData(string directory)
        {
            Directory.CreateDirectory(directory);
            var converter = new InibinConverter(ConversionMap.FromHashCollection(_draft));

            foreach (var file in _files)
            {
                var inibin = _manager.ReadInibin(file.FullName);
                var item = ContentFile.FromInibin(inibin, converter);
                var orgPath = Path.GetDirectoryName(file.FullName);
                var orgName = Path.GetFileNameWithoutExtension(file.FullName);
                Directory.CreateDirectory(directory+"/"+orgPath);
                var savePath = directory + "/" + orgPath + "/" + orgName + ".json";
                File.WriteAllText(savePath, item.Serialize());
            }
        }

        public int CountStrings()
        {
            return _strings.Count;
        }
        
        public int CountHashes()
        {
            return _hashes.Count;
        }

        public int CountDraftHashes()
        {
            return _draft.Hashes.Count;
        }

        public int CountDraftSections()
        {
            int s = 0;
            foreach(var hash in _draft.Hashes)
            {
                s += hash.Value.Count;
            }
            return s;
        }

        public int CountDraftNames()
        {
            int s = 0;
            foreach (var hash in _draft.Hashes)
            {
                foreach(var section in hash.Value)
                {
                    s += section.Value.Count;
                }
            }
            return s;
        }

        public int CountMatches()
        {
            int m = 0;
            foreach (var hash in _draft.Hashes)
            {
                if (_hashes.Contains(hash.Key))
                    m++;
            }
            return m;
        }

        public int CountFiles()
        {
            return _files.Count;
        }

        public int TestHash(string section, string name)
        {
            Console.WriteLine("Testing: " + section + "*" + name);
            var hash = HashFunctions.GetInibinHash(section, name);
            if (_hashes.Contains(hash))
            {
                if (_draft.Hashes.ContainsKey(hash))
                {
                    if (_draft.Hashes[hash].Keys.ToList().Contains(section, StringComparer.OrdinalIgnoreCase))
                    {
                        if (_draft.Hashes[hash][section].Contains(name, StringComparer.OrdinalIgnoreCase))
                        {
                            return 3;
                        }
                    }
                    return 2;
                }
                return 1;
            }
            return 0;
        }

        public void AddHash(string section, string name)
        {
            _draft.AddFromSource(section, name);
            AddString(name);
            AddString(section);
        }

        public void AddString(string name)
        {
            if (!_strings.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                _strings.Add(name);
            }
        }

        public int TestAddHash(string section, string name, bool noConflicts)
        {
            var res = TestHash(section, name);
            switch (res)
            {
                case 0:
                    Console.WriteLine("Not found!");
                    break;
                case 1:
                    Console.WriteLine("Found & added!");
                    AddHash(section, name);
                    break;
                case 2:
                    if (noConflicts)
                    {
                        Console.WriteLine("Conflict - only srings added!");
                        AddString(name);
                        AddString(section);
                    }
                    else
                    {
                        Console.WriteLine("Conflict & added!");
                        AddHash(section, name);
                    }
                    break;
                case 3:
                    Console.WriteLine("Already present!");
                    break;
            }
            return res;
        }

        public void PrintFiles()
        {
            foreach(var file in _files)
            {
                Console.WriteLine(file.FullName);
            }
            Console.WriteLine("Total: " + _files.Count);
        }

        public int ImportMapAsString(string path)
        {
            if (!File.Exists(path))
                return 1;
            var data = File.ReadAllText(path);
            var map = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, uint>>>(data);
            
            foreach(var section in map)
            {
                AddString(section.Key);
                foreach(var name in section.Value)
                {
                    AddString(name.Key);
                }
            }
            return 0;
        }

        public int ImportMapAsDraft(string path)
        {
            if (!File.Exists(path))
                return 1;
            var map = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, uint>>>(File.ReadAllText(path));

            foreach (var section in map)
            {
                foreach (var name in section.Value)
                {
                    _draft.AddFromSource(section.Key, name.Key);      
                }
            }
            return 0;
        }

        public int ImportDraft(string path)
        {
            if (!File.Exists(path))
                return 1;
            var data = File.ReadAllText(path);
            var map = JsonConvert.DeserializeObject<LeagueHashCollection>(data);

            foreach(var hash in map.Hashes)
            {
                foreach(var section in hash.Value) {
                    
                    foreach(var name in section.Value)
                    {
                        _draft.AddFromSource(section.Key, name);
                    }
                }
            }
            return 0;
        }

        public void ClearDraft()
        {
            _draft.Hashes.Clear();
        }

        public void PrintConflict()
        {
            int c = 0;
            foreach(var hash in _draft.Hashes)
            {
               
                if (hash.Value.Count > 1)
                {
                    Console.WriteLine("Conflict in hash: " + hash.Key);
                    c++;
                }
                else
                {
                    foreach (var section in hash.Value)
                    {
                        if (section.Value.Count > 1)
                        {
                            c++;
                            Console.WriteLine("Conflict in hash: " + hash.Key);
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("Conflicts: " + c);
        }

        public void DraftMatch()
        {
            var map = new LeagueHashCollection();
            Console.WriteLine("Starting count: " +_draft.Hashes.Count);
            foreach(var hash in _draft.Hashes)
            {
                if (_hashes.Contains(hash.Key))
                {
                    map.Hashes.Add(hash.Key, hash.Value);
                }
            }
            Console.WriteLine("End count: " + map.Hashes.Count);
            _draft = map;
        }

        public void UnduplicateDraft()
        {
            var map = new LeagueHashCollection();
            foreach (var hash in _draft.Hashes)
            {
                var section2 = new Dictionary<string, HashSet<string>>();
                foreach (var section in hash.Value)
                {
                    if (!section2.Keys.ToList().Contains(section.Key,StringComparer.OrdinalIgnoreCase))
                    {
                        var names = new HashSet<string>();
                        var names2 = new List<string>();
                        foreach(var name in section.Value)
                        {
                            if (!names2.Contains(name, StringComparer.OrdinalIgnoreCase))
                            {
                                names.Add(name);
                                names2.Add(name);
                            }
                        }
                        section2.Add(section.Key, names);
                    }
                }
                map.Hashes.Add(hash.Key, section2);
            }
            _draft = map;
        }


        public string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
         
        public List<string> FillRange(string str,int from, int to)
        {
            List<string> result = new List<string>();
            var count = Regex.Matches(str, "%d").Count;
            if(count == 0)
            {
                result.Add(str);
                return result;
            }else{
                for(int i = from; i < to; i++)
                {
                    result.Add(ReplaceFirst(str, "%d", i.ToString()));
                }
                return FillRange(result, from, to);
            }
        }

        public List<string> FillRange(List<string> strs, int from,int to)
        {
            List<string> result = new List<string>();
            foreach (string str in strs)
            {
                var r = FillRange(str, from, to);
                result.AddRange(r);
            }
            return result;
        }

        
        public void RangeTest(string section, string name, int from, int to,bool noconflicts)
        {
            List<string> sections = FillRange(section, from, to);
            List<string> names = FillRange(name, from, to);
            int[] rs = { 0,0,0,0};
            int c = 0;

            foreach(var s in sections)
            {
                foreach(var n in names)
                {
                    rs[TestAddHash(s, n, noconflicts)]++;
                    c++;
                }
            }
            Console.WriteLine("Not found: " + rs[0]);
            Console.WriteLine("Found & added: " + rs[1]);
            Console.WriteLine("Conflict: " + rs[2]);
            Console.WriteLine("Already present: " + rs[3]);
            Console.WriteLine("Total: " + c);
        }

        public int ExportToMap(string target)
        {
            var mapping = new Dictionary<string, Dictionary<string, uint>>();
            foreach (var entry in _draft.Hashes)
            {
                var hash = entry.Key;
                var section = entry.Value.First();
                var name = section.Value.First();

                if (!mapping.ContainsKey(section.Key)) mapping[section.Key] = new Dictionary<string, uint>();
                mapping[section.Key].Add(name, hash);
            }

            var mappingJson = JObject.FromObject(mapping);
            Program.SanitizeAndSort(mappingJson);
            File.WriteAllText(target,JsonConvert.SerializeObject(mappingJson, Formatting.Indented));
            return 0;
        }

        public void FindFiles(string section, string name)
        {
            uint hash = HashFunctions.GetInibinHash(section, name);
            foreach(var entry in _files)
            {
                var file = _manager.ReadFile(entry.FullName).Uncompress();
                var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                foreach (var kvp in inibin.Content)
                {
                    if(kvp.Key == hash)
                        Console.WriteLine("{0}: {1}", entry.FullName, kvp.Value.Value.ToString());
                }
            }
        }

        public void FindValueType(int type,bool filter)
        {
            foreach(var entry in _files)
            {
                var file = _manager.ReadFile(entry.FullName).Uncompress();
                var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                foreach (var kvp in inibin.Content)
                {

                    if (kvp.Value.Type != type)
                        continue;
                    var fileName = entry.FullName;
                    var hash = kvp.Key.ToString();
                    var value = kvp.Value.Value.ToString();
                    if (_draft.Hashes.ContainsKey(kvp.Key))
                    {
                        var section = _draft.Hashes[kvp.Key].First().Key.ToString();
                        var name = _draft.Hashes[kvp.Key].First().Value.First().ToString();
                        Console.WriteLine("{0} ({1}) \"{2}\": {3}*{4}",
                            fileName, hash, value, section,name);
                    }
                    else if (!filter)
                    {
                        Console.WriteLine("{0} ({1}) \"{2}\"",
                            fileName, hash, value);
                    }
                }
            }
        }

        public int ComparetToMap(string target, string output)
        {
            if (!File.Exists(target))
                return -3;

            var data = File.ReadAllText(target);
            var nmap = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, uint>>>(data);

            var mapping = new Dictionary<string, Dictionary<string, uint>>();
            foreach (var entry in _draft.Hashes)
            {
                var hash = entry.Key;
                var section = entry.Value.First();
                var name = section.Value.First();

                if (!mapping.ContainsKey(section.Key)) mapping[section.Key] = new Dictionary<string, uint>();
                mapping[section.Key].Add(name, hash);
            }
            return 0;

        }

        public void ContentDataMake()
        {
            var localization = FontConfigFile.Load(_manager, "en_US");
            var exporter = new InibinExporter(_manager);

            var itemConfiguration = new ContentConfiguration.Item(localization);
            var spellConfiguration = new ContentConfiguration.Spell(localization);

            exporter.Export(itemConfiguration);
            exporter.Export(spellConfiguration);
        }


    }
}
