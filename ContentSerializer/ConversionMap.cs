using LeagueLib.Files;
using LeagueSandbox.ContentSerializer.HashForce;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer
{
    public class ConversionMap
    {
        private Dictionary<InibinKey, uint> _keyToHash;
        private Dictionary<uint, InibinKey> _hashToKey;

        private ConversionMap()
        {
            _keyToHash = new Dictionary<InibinKey, uint>();
            _hashToKey = new Dictionary<uint, InibinKey>();
        }

        private void Add(InibinKey key, uint hash)
        {
            _keyToHash.Add(key, hash);
            _hashToKey.Add(hash, key);
        }

        public uint GetHash(InibinKey key)
        {
            if(!_keyToHash.ContainsKey(key)) throw new NotSupportedException();
            return _keyToHash[key];
        }

        public InibinKey GetKey(uint hash)
        {
            if (!_hashToKey.ContainsKey(hash))
            {
                var key = new InibinKey("UNKNOWN_HASHES", hash.ToString());
                _keyToHash.Add(key, hash);
                _hashToKey.Add(hash, key);
            }
            return _hashToKey[hash];
        }

        public static ConversionMap Load(string path)
        {
            var result = new ConversionMap();
            var map = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, uint>>>(File.ReadAllText(path));
            foreach(var kvp in map)
            {
                foreach(var entry in kvp.Value)
                {
                    var key = new InibinKey(kvp.Key, entry.Key);
                    var hash = entry.Value;
                    result.Add(key, hash);
                }
            }
            return result;
        }

        public static ConversionMap FromHashCollection(LeagueHashCollection col)
        {
            var result = new ConversionMap();
            var mapping = new Dictionary<string, Dictionary<string, uint>>();
            foreach (var entry in col.Hashes)
            {
                var hash = entry.Key;
                var section = entry.Value.First();
                var name = section.Value.First();
                if (!mapping.ContainsKey(section.Key)) mapping[section.Key] = new Dictionary<string, uint>();
                mapping[section.Key].Add(name, hash);
            }

            foreach (var kvp in mapping)
            {
                foreach (var entry in kvp.Value)
                {
                    var key = new InibinKey(kvp.Key, entry.Key);
                    var hash = entry.Value;
                    result.Add(key, hash);
                }
            }
            return result;
        }

    }

    public class InibinConverter
    {
        private ConversionMap _conversionMap;
        private Dictionary<InibinKey, InibinValue> _keyToValue;
        private Dictionary<uint, InibinValue> _hashToValue;

        public InibinConverter(ConversionMap conversionMap)
        {
            _conversionMap = conversionMap;
            _keyToValue = new Dictionary<InibinKey, InibinValue>();
            _hashToValue = new Dictionary<uint, InibinValue>();
        }

        public void Clear()
        {
            _keyToValue = new Dictionary<InibinKey, InibinValue>();
            _hashToValue = new Dictionary<uint, InibinValue>();
        }

        public void AddByHash(uint hash, InibinValue value)
        {
            var key = _conversionMap.GetKey(hash);
            _keyToValue.Add(key, value);
            _hashToValue.Add(hash, value);
        }

        public void AddByKey(InibinKey key, InibinValue value)
        {
            var hash = _conversionMap.GetHash(key);
            _hashToValue.Add(hash, value);
            _keyToValue.Add(key, value);
        }

        public Dictionary<string, Dictionary<string, object>> Deserialize()
        {
            var result = new Dictionary<string, Dictionary<string, object>>();
            foreach(var entry in _keyToValue)
            {
                var section = entry.Key.Section;
                var name = entry.Key.Name;
                var value = entry.Value;
                if (!result.ContainsKey(section))
                    result[section] = new Dictionary<string, object>();
                result[section].Add(name, value.Value);
            }
            return result;
        }

        public Dictionary<string, Dictionary<string, object>> Convert(Inibin inibin)
        {
            Clear();
            foreach (var kvp in inibin.Content)
            {
                AddByHash(kvp.Key, kvp.Value);
            }
            return Deserialize();
        }

        public Dictionary<uint, InibinValue> Serialize()
        {
            var result = new Dictionary<uint, InibinValue>();
            foreach(var entry in _hashToValue)
            {
                result.Add(entry.Key, entry.Value);
            }
            return result;
        }
    }

    public class InibinKey
    {
        public string Section { get; private set; }
        public string Name { get; private set; }

        public InibinKey(string section, string name)
        {
            Section = section;
            Name = name;
        }
    }
}
