using NLua;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.ContentSerializer
{
    public class LuaDumper
    {
        private Lua _lua;
        private LuaFunction _getstuff;
        private LuaFunction _cleartemp;
        private LuaFunction _gettemp;

        public LuaDumper()
        {
            _lua = new Lua();
            _lua.DoString(@"
                _tmp = {}
                function CreateChildTurret(name, skin, team, index, lane)
                    _tmp[#_tmp + 1] = {
                        ['Name'] = name,
                        ['Skin'] = skin,
                        ['Team'] = team,
                        ['Index'] = index,
                        ['Lane'] = lane,
                    }
                end
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
                    local CreateLevelProps = env['CreateLevelProps']
                    if CreateLevelProps ~= 'CreateLevelProps' then
                        _tmp = {}
                        CreateLevelProps()
                        env['CreateLevelProps'] = _tmp
                    end
                    return env
                end
                envo = {}
            ");
            _getstuff = _lua.GetFunction("GetStuff");
        }
        public Dictionary<string, object> LoadFile(string filepath)
        {
            var f = _lua.LoadFile(filepath);
            return ParseTable(_getstuff.Call(f)[0]);
        }
        public Dictionary<string, object> LoadString(string data)
        {
            var f = _lua.LoadString(data, "");
            return ParseTable(_getstuff.Call(f)[0]);
        }
        public Dictionary<string, object> LoadData(byte[] data)
        {
            var f = _lua.LoadString(data, "");
            return ParseTable(_getstuff.Call(f)[0]);
        }

        private static Dictionary<string, object> ParseTable(object tt)
        {
            var t = (LuaTable)tt;
            var data = new Dictionary<string, object>();
            foreach (KeyValuePair<object, object> kvp in t)
            {
                if (kvp.Value is LuaFunction)
                    continue;
                var key = (string)Convert.ChangeType(kvp.Key, typeof(string), CultureInfo.InvariantCulture);
                data[key] = ParseObject(kvp.Value);
            }
            return data;
        }

        private static object ParseObject(object o)
        {
            return o is LuaTable ? ParseTable(o) : o;
        }
    }
}
