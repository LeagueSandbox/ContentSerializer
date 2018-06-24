using NLua;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer
{
    public static class LuaDumper
    {

        public static Dictionary<string, object> Dump(byte[] data, string name = "")
        {
            var lua = new NLua.Lua();
            lua.DoString(@"
                function Make3DPoint(x,y,z)
                    return { 
                        ['x'] = x, 
                        ['y'] = y,
                        ['z'] = z
                    }
                end
                function GetStuff(func)
                    local env = {}
                    setmetatable(env,  {
                        __index = function(self, key)
                            if _G[key] ~= nil then
                                return _G[key]
                            else
                                return key
                            end
                        end
                    })
                    setfenv(func, env)
                    local status, err = pcall(func)
                    return env
                end
            ");
            var func = lua.LoadString(data, name);
            var table = (LuaTable)(lua.GetFunction("GetStuff").Call(func)[0]);
            var result = new Dictionary<string, object>();
            foreach (KeyValuePair<object, object> kvp in table)
            {
                if (kvp.Value is LuaFunction)
                    continue;
                result[(string)kvp.Key] = ParseObject(kvp.Value);
            }
            return result;
        }

        private static Dictionary<string, object> ParseTableDict(LuaTable table)
        {
            var data = new Dictionary<string, object>();
            foreach (KeyValuePair<object, object> kvp in table)
            {
                if (kvp.Value is LuaFunction)
                    continue;
                var key = (string)Convert.ChangeType(kvp.Key, typeof(string), CultureInfo.InvariantCulture);
                data[key] = ParseObject(kvp.Value);
            }
            return data;
        }

        private static List<object> ParseTableArray(LuaTable table)
        {
            var data = new object[table.Keys.Count];
            foreach (KeyValuePair<object, object> kvp in table)
            {
                var key = (int)Convert.ChangeType(kvp.Key, typeof(double), CultureInfo.InvariantCulture);
                data[key - 1] = kvp.Value;
            }
            return data.ToList();
        }
        private static object ParseObject(object obj)
        {
            if(obj is LuaTable table)
            {
                if(table.Values.Count == 0)
                {
                    return null;
                }
                bool isArray = true;
                foreach(var key in table.Keys)
                {
                    if(key is double || key is int || key is float)
                    {
                        continue;
                    }
                    isArray = false;
                    break;
                }
                if(isArray)
                {
                    try
                    {
                        return ParseTableArray(table);
                    }
                    catch(Exception e)
                    {
                        return ParseTableDict(table);
                    }
                }
                else
                {
                    return ParseTableDict(table);
                }
            }
            else if(obj is LuaFunction)
            {
                return null;
            }
            return obj;
        }
    }
}
