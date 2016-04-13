using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer
{

    public class SectionFilterer
    {
        public Dictionary<string, HashSet<string>> CurrentSections { get; private set; }
        public Dictionary<string, HashSet<string>> LastSections { get; private set; }

        public SectionFilterer(Dictionary<string, HashSet<string>> sections)
        {
            CurrentSections = sections;
            LastSections = sections;
        }

        public void ApplyFilter(HashCollectionFilter filter)
        {
            LastSections = CurrentSections;
            CurrentSections = filter.Filter(CurrentSections);
            var filterCount = LastSections.Count - CurrentSections.Count;
        }
    }

    public abstract class HashCollectionFilter
    {
        public abstract Dictionary<string, HashSet<string>> Filter(Dictionary<string, HashSet<string>> sections);
    }

    public class SectionFilter : HashCollectionFilter
    {
        protected Dictionary<string, HashSet<string>> _allSections;
        protected Dictionary<string, HashSet<string>> _filteredSections;
        protected int _minimumCount;

        public SectionFilter() { _minimumCount = 1; }

        public override Dictionary<string, HashSet<string>> Filter(Dictionary<string, HashSet<string>> sections)
        {
            _allSections = new Dictionary<string, HashSet<string>>();
            _filteredSections = new Dictionary<string, HashSet<string>>();

            foreach (var section in sections)
            {
                _allSections.Add(section.Key, section.Value);
                SectionPass(section.Key, section.Value);
            }

            var finalSections = new Dictionary<string, HashSet<string>>();
            foreach(var section in _filteredSections)
            {
                if (!(section.Value.Count > 0)) continue;
                finalSections.Add(section.Key, section.Value);
            }
            _filteredSections = finalSections;

            var result = GetSectionResult();
            if (result.Count >= _minimumCount) return result;
            return _allSections;
        }

        protected virtual void SectionPass(string name, HashSet<string> content) { }

        protected virtual Dictionary<string, HashSet<string>> GetSectionResult()
        {
            return _filteredSections;
        }

        public T SetMinimumCount<T>(int value) where T : SectionFilter
        {
            _minimumCount = value;
            return (T)this;
        }
    }

    public class FilterPlaintextSections : SectionFilter
    {
        private Regex _regex;

        public FilterPlaintextSections()
        {
            var pattern = @"^[a-zA-Z0-9_\-\.]+\z";
            _regex = new Regex(pattern, RegexOptions.IgnoreCase);
        }

        protected override void SectionPass(string name, HashSet<string> content)
        {
            if (_regex.IsMatch(name)) _filteredSections.Add(name, content);
        }
    }

    public class FilterDuplicateSections : SectionFilter
    {
        private Dictionary<string, bool> _searchFor;
        private string _singleOut;

        public FilterDuplicateSections()
        {
            _searchFor = new Dictionary<string, bool>()
                {
                    {"data", false },
                    {"Data", false },
                    {"DATA", false }
                };
            _singleOut = "Data";
        }

        protected override void SectionPass(string section, HashSet<string> content)
        {
            if (_searchFor.ContainsKey(section)) _searchFor[section] = true;
            else _filteredSections.Add(section, content);
            if (_singleOut == section) _filteredSections.Add(section, content);
        }

        protected override Dictionary<string, HashSet<string>> GetSectionResult()
        {
            var useFilter = true;
            foreach (var entry in _searchFor)
            {
                useFilter = useFilter && entry.Value;
            }

            if (useFilter) return _filteredSections;
            return _allSections;
        }
    }

    public class KeyFilter : SectionFilter
    {
        protected HashSet<string> _allContents;
        protected HashSet<string> _filteredContents;

        protected override void SectionPass(string name, HashSet<string> content)
        {
            _allContents = new HashSet<string>();
            _filteredContents = new HashSet<string>();
            foreach (var entry in content)
            {
                _allContents.Add(entry);
                ContentPass(entry);
            }
            if (_filteredContents.Count >= _minimumCount) _filteredSections.Add(name, GetContentResult());
        }

        protected virtual void ContentPass(string name) { }

        protected virtual HashSet<string> GetContentResult()
        {
            return _filteredContents;
        }
    }

    public class FilterPlaintextKeys : KeyFilter
    {
        private Regex _regex;

        public FilterPlaintextKeys()
        {
            var pattern = @"^[a-zA-Z0-9_\-\.]+\z";
            _regex = new Regex(pattern, RegexOptions.IgnoreCase);
        }

        protected override void ContentPass(string name)
        {
            if (_regex.IsMatch(name)) _filteredContents.Add(name);
        }
    }
}
