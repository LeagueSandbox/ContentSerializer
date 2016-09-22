using LeagueLib.Files;
using LeagueLib.Tools;
using LeagueLib.Hashes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.IO;

namespace LeagueSandbox.ContentSerializer
{
    class ProgramInterfaceCLI
    {
        

        private ProgramInterface prog = new ProgramInterface();
        private Dictionary<string, string> paterns = new Dictionary<string, string>
        {
            {"items", @"Data\/Items\/([^\/]+)\.inibin"},
            {"itemgroups", @"Data\/Items\/ItemGroups\/([^\/]+)\.inibin"},
            {"itemmetadata", @"Data[\/\\]Items[\/\\]metadata[\/\\](.*).inibin"},
            {"spells", @"Data(|\/Characters\/([^\/]+)|\/Shared)\/Spells\/([^\/]+)\.inibin"},
            {"characters", @"Data\/Characters\/([^\/]+)(\/\1\.inibin)"},
            {"talents", @"Data\/Talents\/([^\/]+)\.inibin"},
            {"skins", @"Data\/Characters\/([^\/]+)\/Skins\/([^\/]+)\/\2\.inibin"},
            {"all", @"(.*)\.inibin"}
        };

        public int LoadPatern(string filename)
        {
            if (!File.Exists(filename))
                return -3;
            var pat = File.ReadAllText(filename);
            if (!IsValidRegex(pat))
                return -3;
            prog.AddPatern(pat);
            return 0;
        }

        public ProgramInterfaceCLI()
        {

        }

        public void ConsoleInterface()
        {
            Command("setrads");
            while (true)
            {
                Console.Write("Command: ");
                switch (Command(Console.ReadLine()))
                {
                    case 1:
                        Console.WriteLine("Command error!");
                        break;
                    case -1:
                        Console.WriteLine("Invalid argument count!");
                        break;
                    case -2:
                        Console.WriteLine("Invalid command");
                        break;
                    case -3:
                        return;
                }
            }
        }

        public int Command(string cmds)
        {
            return Command(cmds.Split(' '));
        }

        public int Command(string[] cmd)
        {
            int len = cmd.Length;
            if (len == 0)
                return 0;
            switch (cmd[0])
            {
                case "exit":
                case "quit":
                case "close":
                    return -3;
                case "setrads":
                    if (len > 2) return -1;
                    prog.LoadManager(GetPath(len == 2 ? cmd[1] : ""));
                    break;
                case "importstrings":
                    if (len != 2) return -1;
                    return prog.ImportStrings(cmd[1]);
                case "addpatern":
                    if (len != 2) return -1;
                    SelectPatern(cmd[1]);
                    break;
                case "addregex":
                    if (len != 2) return -1;
                    if (IsValidRegex(cmd[1]))
                    {
                        prog.AddPatern(cmd[1]);
                    }
                    break;
                case "exportdraft":
                    if (len != 2) return -1;
                    prog.ExportDraft(cmd[1]);
                    break;
                case "exportdata":
                    if (len != 2) return -1;
                    prog.ExportData(cmd[1]);
                    break;
                case "exportstrings":
                    if (len != 2) return -1;
                    prog.ExportStrings(cmd[1]);
                    break;
                case "hash":
                    if (len != 1) return -1;
                    prog.HashStrings();
                    break;
                case "clearstrings":
                    if (len != 1) return -1;
                    prog.ClearStrings();
                    break;
                case "syncstrings":
                    if (len != 1) return -1;
                    prog.SyncStrings();
                    break;
                case "importdraftstrings":
                    if (len != 2) return -1;
                    return prog.ImportDraftStrings(cmd[1]);
                case "count":
                    if (len > 2) return -1;
                    CountStuff(len == 2 ? cmd[1] : "all");
                    break;
                case "test":
                    if (len != 3) return -1;
                    prog.TestAddHash(cmd[1], cmd[2], false);
                    break;
                case "ctest":
                    if (len != 3) return -1;
                    prog.TestAddHash(cmd[1], cmd[2], true);
                    break;
                case "listfiles":
                    if (len != 1) return -1;
                    prog.PrintFiles();
                    break;
                case "importmapstrings":
                    if (len != 2) return -1;
                    return prog.ImportMapAsString(cmd[1]);
                case "importmapdraft":
                    if (len != 2) return -1;
                    return prog.ImportMapAsDraft(cmd[1]);
                case "importdraft":
                    if (len != 2) return -1;
                    return prog.ImportDraft(cmd[1]);
                case "exportmap":
                    if (len != 2) return -1;
                    return prog.ExportToMap(cmd[1]);
                case "cleardraft":
                    if (len != 1) return -1;
                    prog.ClearDraft();
                    break;
                case "conflict":
                    if (len != 1) return -1;
                    prog.PrintConflict();
                    break;
                case "draftmatch":
                    if (len != 1) return -1;
                    prog.DraftMatch();
                    break;
                case "unduplicatedraft":
                    if (len != 1) return -1;
                    prog.UnduplicateDraft();
                    break;
                case "clearfiles":
                    if (len != 1) return -1;
                    prog.ClearFiles();
                    break;
                case "listpaterns":
                    if (len != 1) return -1;
                    prog.ListPaterns();
                    break;
                case "delpatern":
                    if (len != 2) return -1;
                    prog.RemovePatern(int.Parse(cmd[1]));
                    break;
                case "rtest":
                    if (len != 5) return -1;
                    prog.RangeTest(cmd[1], cmd[2], Int32.Parse(cmd[3]), Int32.Parse(cmd[4]), false);
                    break;
                case "findfiles":
                    if (len != 3) return -1;
                    prog.FindFiles(cmd[1], cmd[2]);
                    break;
                case "loadpatern":
                    if (len != 2) return -1;
                    return LoadPatern(cmd[1]);
                case "help":
                    Console.WriteLine("Command/Altcommand [argument] [*optional] - descirpiton: ");
                    Console.WriteLine("---------------------------------------------------------");
                    Console.WriteLine("-------Misc:");
                    Console.WriteLine("setrads [*directory] - Set rads path");
                    Console.WriteLine("exit/quit/close - Exits CLI");
                    Console.WriteLine("count [*all|strings,hashes,draft,files] - Counts stuff (default all)");
                    Console.WriteLine("-------Files:");
                    Console.WriteLine("loadpatern [file] - Patern from file");
                    Console.WriteLine("addpatern [patern] - Set inibin match patern");
                    Console.WriteLine("addregex [regex] - Set inibin match regex");
                    Console.WriteLine("listfiles - Prints loaded file list");
                    Console.WriteLine("listpaterns - Prints paterns index list");
                    Console.WriteLine("delpatern [index] - Removes patern at index");
                    Console.WriteLine("clearfiles - Clears files, hashes & paterns");
                    Console.WriteLine("exportdata [directory] - Export data");
                    Console.WriteLine("-------Strings:");
                    Console.WriteLine("importstrings [file] - Adds string list");
                    Console.WriteLine("importdraftstrings [file] - Import strings from given draft file");
                    Console.WriteLine("importmapstrings [file] - Adds strings from conversion map");
                    Console.WriteLine("exportstring [file] - Exports strings");
                    Console.WriteLine("clearstrings - Clears string list");
                    Console.WriteLine("syncstrings - Syncs strings to current draft");
                    Console.WriteLine("-------Draft:");
                    Console.WriteLine("hash - Hashes strings to draft");
                    Console.WriteLine("importdraft [file] - Adds draft");
                    Console.WriteLine("importmapdraft [file] - Adds draft from conversion map");
                    Console.WriteLine("exportdraft [file] - Save draft file");
                    Console.WriteLine("exportmap [file] - Export Conversion map");
                    Console.WriteLine("cleardraft - Clears draft");
                    Console.WriteLine("-------Draft Manual:");
                    Console.WriteLine("test [name] [section] - Test for hash and add it to draft/strings");
                    Console.WriteLine("ctest [name] [section] - Same as Test but avoids conflicts");
                    Console.WriteLine("rtest [name] [section] [from] [to] - Brute force %d fromated string in given range(multiple supported)");
                    Console.WriteLine("conflict - Prints conflicts");
                    Console.WriteLine("draftmatch - Leaves only hashes that match");
                    Console.WriteLine("unduplicatedraft - Leaves only non duplciate strings");
                    Console.WriteLine("findfiles - Finds files");
                    Console.WriteLine("---------------------------------------------------------");
                    break;
                default:
                    return -2;
            }
            return 0;
        }

        private string GetPath(string path)
        {
            if (path != "") return path;
            if (File.Exists("rads-path.txt")) return File.ReadAllText("rads-path.txt");
            Console.WriteLine("Enter rads path: ");
            return Console.ReadLine();
        }

        private void SelectPatern(string patern)
        {

            if (patern == "" || patern == "help" || patern == "list")
            {
                Console.Write("Patern names: ");
                Console.WriteLine(string.Join(", ", paterns.Keys));
                Console.Write("Regex or name: ");
                patern = Console.ReadLine();
            }
            else if (paterns.ContainsKey(patern))
            {
                prog.AddPatern(paterns[patern]);
            }
            else
            {
                Console.WriteLine("Invalid patern!");
            }
        }

        private void CountStuff(string what)
        {
            if (what == "" || what == "all")
                what = "strings,hashes,draft,files";

            if (what.Contains("strings"))
                Console.WriteLine("Strings: " + prog.CountStrings());

            if (what.Contains("hashes"))
                Console.WriteLine("Inibin hashes: " + prog.CountHashes());

            if (what.Contains("files"))
                Console.WriteLine("Files: " + prog.CountFiles());

            if (what.Contains("draft"))
            {
                Console.WriteLine("Draft hashes: " + prog.CountDraftHashes());
                Console.WriteLine("Draft sections: " + prog.CountDraftSections());
                Console.WriteLine("Draft names: " + prog.CountDraftNames());
                Console.WriteLine("Matching hashes: " + prog.CountMatches());
            }

        }

        private  bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

    }
}
