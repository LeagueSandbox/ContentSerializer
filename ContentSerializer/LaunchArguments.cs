using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer
{
    public class LaunchArguments
    {
        private Dictionary<string, string> _arguments;

        private LaunchArguments() { _arguments = new Dictionary<string, string>(); }

        /// <summary>
        /// Returns the argument for given key
        /// </summary>
        /// <param name="key">Argument key</param>
        /// <returns></returns>
        public string this[string key]
        {
            get { return _arguments[key]; }
        }

        /// <summary>
        /// Returns wether or a provided key exists
        /// </summary>
        /// <param name="key">Key which existance to check</param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return _arguments.ContainsKey(key);
        }

        /// <summary>
        /// Parses launch arguments into a key/value mapping
        /// </summary>
        /// <param name="arguments">Launch arguments</param>
        /// <returns>LaunchArguments object</returns>
        public static LaunchArguments Parse(string[] arguments)
        {
            var result = new LaunchArguments();

            //Make sure we have a proper amount of arguments, since the format is --<key> <value>
            if (arguments.Length % 2 != 0) throw new Exception("Invalid launch argument count");

            //Form key value pairs
            for (var i = 0; i < arguments.Length;)
            {
                var key = arguments[i++];
                var value = arguments[i++];

                //Make sure proper format is employed
                if (!key.StartsWith("--") || key.Contains(' ')) throw new Exception("Invalid argument format");
                if (key.Length < 3) throw new Exception("Invalid argument key length");
                if (key[2] == '-') throw new Exception("Invalid argument key");

                //Trim leading dashes
                key = key.TrimStart('-');

                result._arguments.Add(key, value);
            }

            return result;
        }
    }
}
