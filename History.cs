using System;
using System.IO;
using System.Collections.Generic;

class History {
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
            Console.Write(added[dat] + "|" + dat);
            if (removed.ContainsKey(dat)) Console.Write("|REMOVED - " + removed[dat]);
            Console.WriteLine();
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
