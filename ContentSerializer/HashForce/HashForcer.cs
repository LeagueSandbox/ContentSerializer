using LeagueLib.Files;
using LeagueLib.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer.HashForce
{
    public class HashForcer
    {
        public int HashCount { get { return _hashes == null ? 0 : _hashes.Count; } }

        private LeagueHashSourceCollection _result;
        private List<HashWorker> _workers;
        private HashSet<uint> _hashes;
        private string[] _stringSources;
        private bool _consoleOutput;

        public HashForcer(bool consoleOutput)
        {
            _consoleOutput = consoleOutput;
        }

        public void LoadHashes(ArchiveFileManager manager)
        {
            UpdateStatus("Loading League hashes...");
            var fileEntries = manager.GetAllFileEntries();
            var hashList = new HashSet<uint>();

            foreach (var entry in fileEntries)
            {
                if (!entry.FullName.Contains(".inibin")) continue;

                var file = manager.ReadFile(entry.FullName).Uncompress();
                var inibin = Inibin.DeserializeInibin(file, entry.FullName);
                foreach (var kvp in inibin.Content)
                {
                    if (hashList.Contains(kvp.Key)) continue;
                    hashList.Add(kvp.Key);
                }
            }

            _hashes = hashList;
        }

        public void LoadSources(string sourcesPath)
        {
            UpdateStatus("Loading strings...");
            if (!sourcesPath.Contains(".json")) throw new Exception("Only json string sources supported for now");

            var stringListJson = JArray.Parse(File.ReadAllText(sourcesPath));
            var stringList = new HashSet<string>();

            foreach(JValue value in stringListJson)
            {
                var sValue = (string)value.Value;
                if (string.IsNullOrEmpty(sValue)) continue;
                if (stringList.Contains(sValue)) continue;
                stringList.Add(sValue);
            }

            _stringSources = stringList.ToArray();
        }

        public void Run(int workerCount)
        {
            UpdateStatus("Checking for matches...");
            if (_stringSources == null) throw new Exception("String sources must be specified");
            if (_hashes == null) throw new Exception("Hashes must be specified");
            if (workerCount < 1) throw new Exception("At least one worker must be present");

            var countPerWorker = (int)Math.Ceiling((decimal)_stringSources.Length / workerCount);
            _workers = new List<HashWorker>(workerCount);

            for (int i = 0; i < _stringSources.Length; i+= countPerWorker)
            {
                var worker = new HashWorker(_stringSources, _hashes, i, countPerWorker);
                _workers.Add(worker);
                worker.Start();
            }
        }

        public void WaitFinish()
        {
            long progress = 0;
            long totalProgress = _stringSources.LongLength * _stringSources.LongLength;
            while (progress < totalProgress)
            {
                progress = 0;
                for (var i = 0; i < _workers.Count; i++)
                {
                    progress += _workers[i].Progress;
                }
                var percentage = (float)(progress + 1) / totalProgress * 100;
                UpdateStatus(string.Format("\rProgress: {0:00.00} %", percentage), false);
                Thread.Sleep(20);
            }
            UpdateStatus("");
            UpdateStatus("All workers finished");
        }

        public LeagueHashSourceCollection GetResult()
        {
            if (_result != null) return _result;
            _result = new LeagueHashSourceCollection();

            foreach(var worker in _workers)
            {
                _result.Combine(worker.GetResult());
            }

            return _result;
        }

        private void UpdateStatus(string message)
        {
            UpdateStatus(message, true);
        }

        private void UpdateStatus(string message, bool newline)
        {
            if (!_consoleOutput) return;
            if (newline) Console.WriteLine(message);
            else Console.Write(message);
        }
    }

    public class HashWorker
    {
        private LeagueHashSourceCollection _result;
        private Thread _workThread;
        private HashSet<uint> _hashes;
        private string[] _sources;
        private int _start;
        private int _count;
        private bool _finished;

        public int Progress { get; private set; }
        public bool IsFinished { get { return _finished; } }

        public HashWorker(string[] sources, HashSet<uint> hashes, int start, int count)
        {
            _sources = sources;
            _hashes = hashes;
            _start = start;
            _count = count;
            _finished = false;
        }

        public void Start()
        {
            _workThread = new Thread(Work);
            _workThread.Start();
        }

        public LeagueHashSourceCollection GetResult()
        {
            if (!_finished) throw new Exception("The worker hasn't finished yet");
            return _result;
        }

        private void Work()
        {
            _finished = false;
            _result = new LeagueHashSourceCollection();
            Progress = 0;
            var end = _start + _count;
            for(var i = _start; i < end && i < _sources.Length; i++)
            {
                for(var j = 0; j < _sources.Length; j++)
                {
                    var hash = LeagueLib.Hashes.HashFunctions.GetInibinHash(_sources[i], _sources[j]);
                    if (_hashes.Contains(hash)) _result.AddSource(_sources[i], _sources[j]);
                    Progress++;
                }
            }
            _finished = true;
        }
    }

    public class LeagueHashSourceCollection
    {
        public int SectionCount { get { return _content.Count; } }
        public int HashCount { get; private set; }
        public Dictionary<string, HashSet<string>> Content { get { return _content; } }

        private Dictionary<string, HashSet<string>> _content;

        public LeagueHashSourceCollection() { _content = new Dictionary<string, HashSet<string>>(); }

        public void AddSource(string section, string key)
        {
            if (!_content.ContainsKey(section)) _content[section] = new HashSet<string>();
            if (_content[section].Contains(key)) return;
            _content[section].Add(key);
            HashCount++;
        }

        public void Combine(LeagueHashSourceCollection other)
        {
            foreach(var kvp in other._content)
            {
                foreach(var entry in kvp.Value)
                {
                    AddSource(kvp.Key, entry);
                }
            }
        }
    }

    public class LeagueHashCollection
    {
        public Dictionary<uint, Dictionary<string, HashSet<string>>> Hashes { get { return _hashes; } }

        private Dictionary<uint, Dictionary<string, HashSet<string>>> _hashes;

        public LeagueHashCollection() { _hashes = new Dictionary<uint, Dictionary<string, HashSet<string>>>(); }

        public LeagueHashCollection(LeagueHashSourceCollection hashSources)
        {
            _hashes = new Dictionary<uint, Dictionary<string, HashSet<string>>>();
            foreach (var kvp in hashSources.Content)
            {
                foreach (var name in kvp.Value)
                {
                    AddFromSource(kvp.Key, name);
                }
            }
        }

        public void AddFromSource(string section, string name)
        {
            var hash = LeagueLib.Hashes.HashFunctions.GetInibinHash(section, name);
            if (!_hashes.ContainsKey(hash)) _hashes[hash] = new Dictionary<string, HashSet<string>>();
            if (!_hashes[hash].ContainsKey(section)) _hashes[hash][section] = new HashSet<string>();
            _hashes[hash][section].Add(name);
        }
    }
}
