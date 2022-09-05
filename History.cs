using System;
using System.IO;
using System.Collections.Generic;
using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;

class History {

    public static Dictionary<string, string> BuildMonsterVarietyHistory(bool hideVersionName = false) {
        Dictionary<string, int> monsterCounts = new Dictionary<string, int>();
        Dictionary<string, string> added = new Dictionary<string, string>();
        Dictionary<string, string> removed = new Dictionary<string, string>();
        DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemamin.txt");
        
        foreach (string dir in Directory.EnumerateDirectories(@"F:\Extracted\PathOfExile")) {
            HashSet<string> prevMonsters = new HashSet<string>(added.Keys);
            string version = Path.GetFileName(dir);
            if (!char.IsDigit(version[0])) continue;
            if (hideVersionName) version = version.Substring(0, version.LastIndexOf('.'));
            monsterCounts[version] = 0;
            DatFileIndex dats = new DatFileIndex(new DiskDirectory(Path.Combine(dir, "ROOT/Data")), spec);
            string datName = dats.ContainsKey("MonsterVarieties.dat64") ? "MonsterVarieties.dat64" : "MonsterVarieties.dat";
            foreach (DatRow row in dats[datName]) {
                string monster = row["Id"].GetString().TrimEnd('_');
                prevMonsters.Remove(monster);
                if (!added.ContainsKey(monster)) { added[monster] = version; monsterCounts[version] = monsterCounts[version] + 1; }
            }
            foreach (string removedMonster in prevMonsters) if (!removed.ContainsKey(removedMonster)) removed[removedMonster] = version;
        }
        /*
        foreach (string monster in added.Keys) {
            if (removed.ContainsKey(monster)) Console.WriteLine($"{added[monster]}|{monster}|{removed[monster]}");
            else Console.WriteLine(added[monster] + "|" + monster);
        }
        */
        foreach (string version in monsterCounts.Keys) Console.WriteLine(version + "|" + monsterCounts[version].ToString());
        return added;
    }

    public static void DatHistory() {
        Dictionary<string, string> added = new Dictionary<string, string>();
        Dictionary<string, string> removed = new Dictionary<string, string>();

        foreach (string dir in Directory.EnumerateDirectories(@"F:\Extracted\PathOfExile")) {
            HashSet<string> prevDats = new HashSet<string>(added.Keys);
            string version = Path.GetFileName(dir);
            if (!char.IsDigit(version[0])) continue;
            foreach (string dat in Directory.EnumerateFiles(Path.Combine(dir, "ROOT/Data"), "*.dat")) {
                string datname = Path.GetFileName(dat);
                prevDats.Remove(datname);
                if(!added.ContainsKey(datname)) added[datname] = version;
            }
            foreach (string removedDat in prevDats) if(!removed.ContainsKey(removedDat)) removed[removedDat] = version;
        }
        foreach (string dat in added.Keys) {
            if (removed.ContainsKey(dat)) Console.WriteLine($"{added[dat]} - {removed[dat]}|{dat}");
            else Console.WriteLine(added[dat] + "|" + dat);
        }

    }

    public static void GGPKSize() {
        foreach (string dir in Directory.EnumerateDirectories(@"F:\Extracted\PathOfExile")) {
            string version = Path.GetFileName(dir);
            if (!char.IsDigit(version[0])) continue;
            FileInfo info = new FileInfo(Path.Combine(dir, "8855727/Content.ggpk"));
            Console.WriteLine(String.Format("{0},{1:F2}", version, info.Length / 1024f / 1024f / 1024f));
        }
    }
}
