using LeagueSandbox.ContentSerializer.HashForce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer
{
    public class ConflictResolver
    {
        public ConflictResolver() { }

        public LeagueHashCollection ResolveConflicts(LeagueHashCollection hashCollection)
        {
            var result = new LeagueHashCollection();
            foreach(var hash in hashCollection.Hashes)
            {
                var sections = hash.Value;
                if(sections.Count > 1)
                {
                    sections = ResolveConflicts(sections);
                }
                result.Hashes.Add(hash.Key, sections);
            }
            return result;
        }

        private Dictionary<string, HashSet<string>> ResolveConflicts(Dictionary<string, HashSet<string>> sections)
        {
            var options = new List<KeyValuePair<string, HashSet<string>>>();
            foreach(var section in sections)
            {
                var content = section.Value;
                if(content.Count > 1)
                {
                    content = ResolveConflicts(section.Key, content);
                }
                options.Add(new KeyValuePair<string, HashSet<string>>(section.Key, content));
            }

            Console.Clear();
            var result = new Dictionary<string, HashSet<string>>();
            for (var i = 0; i < options.Count; i++)
            {
                foreach (var entry in options[i].Value)
                {
                    Console.WriteLine("{0}  -  {1}  -  {2}", i + 1, options[i].Key, entry);
                }
            }
            var input = -1;
            while (input < 0 || input > options.Count)
            {
                try { input = Convert.ToInt32(Console.ReadLine()); }
                catch { }
            }
            if (input == 0) return new Dictionary<string, HashSet<string>>();

            var selection = options[input - 1];
            return new Dictionary<string, HashSet<string>>() { { selection.Key, selection.Value } };
        }

        private HashSet<string> ResolveConflicts(string section, HashSet<string> content)
        {
            Console.Clear();
            var options = content.ToArray();
            for (var i = 0; i < options.Length; i++)
            {
                Console.WriteLine("{0}  -  {1}  -  {2}", i + 1, section, options[i]);
            }
            var input = -1;
            while(input < 0 || input > options.Length)
            {
                try { input = Convert.ToInt32(Console.ReadLine()); }
                catch { }
            }
            if (input == 0) return new HashSet<string>();
            return new HashSet<string>() { options[input - 1] };
        }
    }
}
