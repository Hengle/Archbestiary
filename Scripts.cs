using Archbestiary.Util;
using ImageMagick;
using PoeSharp.Filetypes.Dat;
using System;
using System.Text;

static class Scripts {

    public static void ListDatRowCounts(Bestiary b) {
        foreach (DatFile dat in b.dats.Values) {
            if(dat.Name.Contains("Abyss")) Console.WriteLine(dat.Name);
            continue;
            if (dat.Name.EndsWith(".dat64"))
                Console.WriteLine($"{dat.RowCount}|{dat.Name}");
        }
    }


    public static void ListUnusedRigs() {
        HashSet<string> rigs = new HashSet<string>(Directory.EnumerateFiles(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\Art\Models", "*.amd", SearchOption.AllDirectories));
        Console.WriteLine("RIG LIST CREATED");
        foreach(string ao in Directory.EnumerateFiles(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\Metadata\", "*.ao", SearchOption.AllDirectories)) {
            string rig = Bestiary.GetRigFromAO("", ao);
            if (!rig.StartsWith("COULD NOT")) {
                string combined = Path.Combine(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\", rig).Replace('/', '\\');
                if(rigs.Contains(combined)) {
                    //Console.WriteLine(combined);
                    rigs.Remove(combined);
                }
            }
        }
        Console.WriteLine("\r\n\r\n\r\n\r\n\r\nUNUSED");
        foreach (string rig in rigs) Console.WriteLine(rig);
    }

    public static void ListIdles() {
        string folder = @"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\Art\Models\";
        List<string> idles = new List<string>();
        foreach (string rig in Directory.EnumerateFiles(folder, "*.amd", SearchOption.AllDirectories)) {
            idles.Clear();
            foreach (string line in File.ReadAllLines(rig)) {
                if (line.Contains("idle", StringComparison.InvariantCultureIgnoreCase)) idles.Add(line);
            }
            if(idles.Count > 1) {
                Console.Write(rig.Substring(folder.Length, rig.Length - folder.Length) + " ");
                foreach (string idle in idles) Console.Write(idle + " ");
                Console.WriteLine();
            }
        }

    }

    public static void ListMonsterRigs(Bestiary b) {
        var history = History.BuildMonsterVarietyHistory();

        for (int i = 0; i < b.dats["MonsterVarieties.dat64"].RowCount; i++) {
            DatRow monster = b.dats["MonsterVarieties.dat64"][i];
            DatReference monsterType = monster["MonsterTypesKey"].GetReference();
            string monsterTypeId = monsterType.GetReferencedRow()["Id"].GetString();
            bool summoned = monsterType.GetReferencedRow()["IsSummoned"].GetPrimitive<bool>();
            string monsterId = monster["Id"].GetString().TrimEnd('_');
            string monsterName = monster["Name"].GetString();
            string version = history.ContainsKey(monsterId) ? history[monsterId] : "unk";
            

            foreach (string ao in monster["AOFiles"].GetStringArray()) {
                string rig = Bestiary.GetRigFromAO(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT", ao);
                Console.WriteLine($"{version}@{monsterType.RowIndex}@{summoned}@{monsterTypeId}@{i}@{monsterId}@{monsterName}@{ao}@{rig}");
                break;
            }
        }
    }


    public static void UniqueList(Bestiary b, int start = 1346) {
        for (int i = start; i < b.dats["UniqueStashLayout.dat64"].RowCount; i++) {
            DatRow unique = b.dats["UniqueStashLayout.dat64"][i];
            string name = unique["WordsKey"].GetReference().GetReferencedRow()["Text"].GetString();
            string icon = unique["ItemVisualIdentityKey"].GetReference().GetReferencedRow()["DDSFile"].GetString();
            string type = unique["UniqueStashTypesKey"].GetReference().GetReferencedRow().GetID();
            Console.WriteLine($"|{i}|{name}|||{type}||||{icon}");
        }
    }

    public static void ActiveSkillTypes(Bestiary b) {
        //Console.Write(";;");
        //for(int i = 0; i < b.dats["ActiveSkillType.dat64"].RowCount; i++) {
        //    Console.Write(b.dats["ActiveSkillType.dat64"][i].GetID() + ";");
        //}
        Console.WriteLine();
        for (int i = 0; i < b.dats["ActiveSkills.dat64"].RowCount; i++) {
            DatRow activeSkill = b.dats["ActiveSkills.dat64"][i];
            HashSet<int> activeSkillTypes = new HashSet<int>();
            foreach (var activeSkillType in activeSkill["ActiveSkillTypes"].GetReferenceArray()) activeSkillTypes.Add(activeSkillType.RowIndex);

            HashSet<int> contextFlags = new HashSet<int>();
            foreach (DatReference contextFlagRef in activeSkill["VirtualStatContextFlags"].GetReferenceArray()) contextFlags.Add(contextFlagRef.RowIndex);
            if (activeSkillTypes.Contains(0) && !contextFlags.Contains(2)) Console.WriteLine(activeSkill.GetID() + " NO ATTACK CONTEXTFLAG");
            if (!activeSkillTypes.Contains(0) && contextFlags.Contains(2)) Console.WriteLine(activeSkill.GetID() + " no skill type");
        }
    }

    public static void UniqueArt(Bestiary b) {
        for (int i = 0; i < b.dats["UniqueStashLayout.dat64"].RowCount; i++) {
            DatRow unique = b.dats["UniqueStashLayout.dat64"][i];
            DatRow visualIdentity = unique["ItemVisualIdentityKey"].GetReference().GetReferencedRow();
            string name = unique["WordsKey"].GetReference().GetReferencedRow()["Text"].GetStringNoNull();
            string type = unique["UniqueStashTypesKey"].GetReference().GetReferencedRow().GetID();
            string mesh1 = visualIdentity["AOFile"].GetStringNoNull();
            string mesh2 = visualIdentity["AOFile2"].GetStringNoNull();
            string[] skinnedMeshes = visualIdentity["MarauderSMFiles"].GetStringArray();
            string skinnedMesh = skinnedMeshes.Length > 0 ? skinnedMeshes[0] : "";
            string effect = visualIdentity["EPKFile"].GetStringNoNull();
            Console.WriteLine($"{i}|{name}|{type}|{mesh1}|{mesh2}|{skinnedMesh}|{effect}");
        }
    }

    public static void UniqueArt2(Bestiary b) {
        HashSet<string> baseAO = new HashSet<string>(File.ReadAllLines(@"E:\Anna\Anna\Visual Studio\Archbestiary\bin\Debug\net6.0\basebases.txt"));
        for (int i = 0; i < b.dats["UniqueStashLayout.dat64"].RowCount; i++) {

            List<string> aoFiles = new List<string>();

            DatRow unique = b.dats["UniqueStashLayout.dat64"][i];
            DatRow visualIdentity = unique["ItemVisualIdentityKey"].GetReference().GetReferencedRow();
            string name = unique["WordsKey"].GetReference().GetReferencedRow()["Text"].GetStringNoNull();
            string type = unique["UniqueStashTypesKey"].GetReference().GetReferencedRow().GetID();


            string mesh1 = visualIdentity["AOFile"].GetStringNoNull(); if (mesh1.EndsWith(".ao") || mesh1.EndsWith(".epk")) aoFiles.Add(mesh1);
            string mesh2 = visualIdentity["AOFile2"].GetStringNoNull(); if (mesh2.EndsWith(".ao") || mesh2.EndsWith(".epk")) aoFiles.Add(mesh2);
            string epk = visualIdentity["EPKFile"].GetStringNoNull(); if (epk.EndsWith(".ao") || epk.EndsWith(".epk")) aoFiles.Add(epk);

            string[] marauderFiles = visualIdentity["MarauderSMFiles"].GetStringArray();
            foreach (string s in marauderFiles) if (s.EndsWith(".ao")) aoFiles.Add(s);
            string[] smFiles = visualIdentity["SMFiles"].GetStringArray();
            foreach (string s in smFiles) if (s.EndsWith(".ao")) aoFiles.Add(s);


            Console.Write($"{i}|{name}|{type}|");
            foreach (string s in aoFiles) {
                if(!baseAO.Contains(s))
                //if (s.EndsWith("Drop.ao") && aoFiles.Count > 1) continue;
                Console.Write(s + "|");
            }
            Console.WriteLine();
        }
    }
    public static void BaseArt2(Bestiary b) {
        for (int i = 0; i < b.dats["BaseItemTypes.dat64"].RowCount; i++) {
            DatRow baseItem = b.dats["BaseItemTypes.dat64"][i];

            List<string> aoFiles = new List<string>();
            DatRow visualIdentity = baseItem["ItemVisualIdentity"].GetReference().GetReferencedRow();
            string name = baseItem["Name"].GetStringNoNull();
            string type = baseItem["ItemClassesKey"].GetReference().GetReferencedRow().GetID();

            string mesh1 = visualIdentity["AOFile"].GetStringNoNull(); if (mesh1.EndsWith(".ao")) aoFiles.Add(mesh1);
            string mesh2 = visualIdentity["AOFile2"].GetStringNoNull(); if (mesh2.EndsWith(".ao")) aoFiles.Add(mesh2);
            string epk = visualIdentity["EPKFile"].GetStringNoNull(); if (epk.EndsWith(".epk")) aoFiles.Add(epk);

            string[] marauderFiles = visualIdentity["MarauderSMFiles"].GetStringArray();
            foreach (string s in marauderFiles) if (s.EndsWith(".ao")) aoFiles.Add(s);
            string[] smFiles = visualIdentity["SMFiles"].GetStringArray();
            foreach (string s in smFiles) if (s.EndsWith(".ao")) aoFiles.Add(s);


            Console.Write($"{type}|{baseItem.GetID()}|{i}|{name}|");
            foreach (string s in aoFiles) {
                //if (s.EndsWith("Drop.ao") && aoFiles.Count > 1) continue;
                Console.Write(s + "|");
            }
            Console.WriteLine();
        }
    }
    public static void BaseArt(Bestiary b) {
        for (int i = 0; i < b.dats["BaseItemTypes.dat64"].RowCount; i++) {
            DatRow baseItem = b.dats["BaseItemTypes.dat64"][i];
            string id = baseItem["Id"].GetStringNoNull();
            string name = baseItem["Name"].GetStringNoNull();
            DatRow visualIdentity = baseItem["ItemVisualIdentity"].GetReference().GetReferencedRow();
            List<string> aoFiles = new List<string>();

            DatRow unique = b.dats["UniqueStashLayout.dat64"][i];

            string mesh1 = visualIdentity["AOFile"].GetStringNoNull(); if (mesh1.EndsWith(".ao")) aoFiles.Add(mesh1);
            string mesh2 = visualIdentity["AOFile2"].GetStringNoNull(); if (mesh2.EndsWith(".ao")) aoFiles.Add(mesh2);

            string[] marauderFiles = visualIdentity["MarauderSMFiles"].GetStringArray();
            foreach (string s in marauderFiles) if (s.EndsWith(".ao")) aoFiles.Add(s);
            string[] smFiles = visualIdentity["SMFiles"].GetStringArray();
            foreach (string s in smFiles) if (s.EndsWith(".ao")) aoFiles.Add(s);

            //Console.Write($"{i}|{name}|{type}");
            foreach (string s in aoFiles) {
                //if (s.EndsWith("Drop.ao") && aoFiles.Count > 1) continue;
                Console.Write(s + "|");
            }
            Console.WriteLine();
        }
    }

    public static void ListMonsterColumns(Bestiary b) {
        var monsterDef = b.spec["MonsterVarieties"];
        for (int i = 0; i < monsterDef.Columns.Count; i++) {
            Console.Write(monsterDef.Columns[i].Name + "|");
        }
        Console.WriteLine();
    }

    public static void ListMonster(Bestiary b, int monsterRow) {
        DatRow monster = b.dats["MonsterVarieties.dat64"][monsterRow];
        var monsterDef = b.spec["MonsterVarieties"];
        for (int i = 0; i < monsterDef.Columns.Count; i++) {
            var col = monsterDef.Columns[i];
            string val = "OOOOO88";
            if (col.Array) {
                if (col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.ForeignRow) {
                    DatReference[]? refs = monster[col.Name].GetReferenceArray();
                    if (refs is null) val = "null";
                    else {
                        val = "<";
                        for (int refIndex = 0; refIndex < refs.Length - 1; refIndex++) {
                            val = val + $"{refs[refIndex].RowIndex}, ";
                        }
                        if (refs.Length > 0) val = val + refs[refs.Length - 1].RowIndex.ToString();
                        val = val + ">";
                    }

                } else if (col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.I32) {
                    val = "";
                    int[] vals = monster[col.Name].GetPrimitiveArray<int>();
                    for (int v = 0; v < vals.Length - 1; v++) val = val + vals[i].ToString() + ", ";
                    if (vals.Length > 0) val = val + vals[vals.Length - 1].ToString();
                } else if (col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.String) {
                    string[] vals = monster[col.Name].GetStringArray();
                    val = "";
                    for (int v = 0; v < vals.Length - 1; v++) val = val + vals[i] + ", ";
                    if (vals.Length > 0) val = val + vals[vals.Length - 1];
                }
            } else if (col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.I32) val = monster[col.Name].GetPrimitive<int>().ToString();
            else if (col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.String) {
                string? s = monster[col.Name].GetString();
                val = s is null ? "null" : s;
            } else if (col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.Bool) val = monster[col.Name].GetPrimitive<bool>().ToString();
            else if (col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.ForeignRow) {
                DatReference? foreignRef = monster[col.Name].GetReference();
                if (foreignRef is null) val = "null";
                else val = $"<{foreignRef.RowIndex}>";
            } else if (col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.F32) val = monster[col.Name].GetPrimitive<float>().ToString();
            if (val == "OOOOO88") Console.WriteLine($"UNK_{col.Type}{(col.Array ? "_ARRAY" : "")}_{(int)col.Type} - {col.Name}  ");
            else Console.Write($"{val}|");
        }
        Console.WriteLine();
    }

    public static void PrintColours(string path = @"F:\Extracted\PathOfExile\3.19.Kalandra\uicolours.txt") {
        MagickImageCollection images = new MagickImageCollection();
        foreach (string line in File.ReadAllLines(path)) {
            string[] words = line.Split('"');
            string[] colournames = words[3].Split(',');
            MagickColor colour = new MagickColor(byte.Parse(colournames[0]), byte.Parse(colournames[1]), byte.Parse(colournames[2]));
            MagickImage image = new MagickImage(colour, 256, 16);
            image.Draw(new Drawables()
                .StrokeColor(MagickColors.White).StrokeWidth(3).Text(4, 11, words[1])
            );
            image.Draw(new Drawables().FillColor(new MagickColor(colour.R, colour.G, colour.B, 130)).Rectangle(0, 0, 256, 16));
            image.Draw(new Drawables()
                .FillColor(MagickColors.Black).Text(4, 11, words[1])
            );
            images.Add(image);
            Console.WriteLine($"{words[1]}   -   {words[3]}");
            //if (images.Count > 10) break;
        }
        var montage = images.Montage(new MontageSettings() { Geometry = new MagickGeometry(256, 16), TileGeometry = new MagickGeometry(1, images.Count) });
        Console.WriteLine($"{montage.Width} {montage.Height}");
        montage.Write(@"F:\Extracted\PathOfExile\3.19.Kalandra\colours.png");
    }

    public static void MonsterBaseStats(Bestiary b) {

        /*
        Console.Write("];\n    const evasionPerLevel = [");
        for (int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.WriteLine(row["UnkEvasion"].GetPrimitive<int>() * 1.0f / row["Evasion"].GetPrimitive<int>());
        }
        */
        Console.Write("    const damagePerLevel = [");
        for (int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row["Damage"].GetPrimitive<float>().ToString() + ",");
        }
        Console.Write("];\n    const lifePerLevel = [");
        for(int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row["Life"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("];\n    const armourPerLevel = [");
        for (int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row["Armour"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("];\n    const evasionPerLevel = [");
        for (int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row["Evasion"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("];\n    const evasiveEvasionPerLevel = [");
        for (int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row["UnkEvasion"].GetPrimitive<int>().ToString() + ",");
        }

        Console.Write("];\n    const fireRes = [[");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["FireNormal"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("], [");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["FireCruel"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("], [");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["FireMerciless"].GetPrimitive<int>().ToString() + ",");
        }

        Console.Write("]];\n    const coldRes = [[");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["ColdNormal"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("], [");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["ColdCruel"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("], [");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["ColdMerciless"].GetPrimitive<int>().ToString() + ",");
        }

        Console.Write("]];\n    const lightningRes = [[");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["LightningNormal"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("], [");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["LightningCruel"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("], [");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["LightningMerciless"].GetPrimitive<int>().ToString() + ",");
        }

        Console.Write("]];\n    const chaosRes = [[");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["ChaosNormal"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("], [");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["ChaosCruel"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("], [");
        for (int i = 0; i < b.dats["MonsterResistances.dat64"].RowCount; i++) {
            DatRow row = b.dats["MonsterResistances.dat64"][i];
            Console.Write(row["ChaosMerciless"].GetPrimitive<int>().ToString() + ",");
        }


        Console.Write("];");

    }
}


/*
 
for (int i = 0; i < dats["DivinationCardStashTabLayout.dat64"].RowCount; i++) {
    DatRow row = dats["DivinationCardStashTabLayout.dat64"][i];
    DatRow baseItem = row["BaseItemTypesKey"].GetReference().GetReferencedRow();
    Console.WriteLine($"{i}|{baseItem.GetName()}");
}
return;

void ArmourSets() {
    HashSet<string> storeArmour = new HashSet<string>();
    foreach(string line in File.ReadAllLines(@"E:\Extracted\PathOfExile\3.18.Sentinel\storeArmour.txt")) {
        if (line.EndsWith(" Body Armour")) storeArmour.Add(line.Substring(0, line.IndexOf(" Body Armour")));
        else if (line.EndsWith(" Body")) storeArmour.Add(line.Substring(0, line.IndexOf(" Body")));
        else storeArmour.Add(line);
    }

    MagickImageCollection montage = new MagickImageCollection();
    int setIndex = 0;
    foreach(DatRow itemizedEffect in dats["ItemisedVisualEffect.dat64"]) {
        foreach(int slot in itemizedEffect["MicrotransactionSlots"].GetPrimitiveArray<int>()) {
            if(slot == 9) { //body
                DatRow baseItem = itemizedEffect["BaseItemTypesKey"].GetReference().GetReferencedRow();
                DatReference visualIdentityRef = baseItem["ItemVisualIdentity"].GetReference();
                if(visualIdentityRef is null) Console.WriteLine(baseItem.GetName() + " - NULL");
                else {
                    DatRow visualIdentity = visualIdentityRef.GetReferencedRow();
                    string name = baseItem.GetName();
                    if (name.EndsWith(" Body Armour")) name = name.Substring(0, name.IndexOf(" Body Armour"));
                    else if (name.EndsWith(" Body")) name = name.Substring(0, name.IndexOf(" Body"));
                    MagickColor bgColor = storeArmour.Contains(name) ? MagickColors.Black : MagickColors.DarkRed ;
                    MagickImage bg = new MagickImage(bgColor, 160, 240);
                    MagickImage image = new MagickImage(Path.Combine(@"F:\Extracted\PathOfExile\ZZZZZZZZZZZZZZZZZZ3.18.Sentinel", visualIdentity["DDSFile"].GetString()));
                    if (image.Height > 240) image.Resize(new MagickGeometry() { Height = 240 });
                    bg.Draw(new Drawables()
                        .Gravity(Gravity.Center)
                        .Composite(0, 0, CompositeOperator.Over, image)
                        .FillColor(MagickColors.White)
                        .Gravity(Gravity.Southwest)
                        .Text(2, 2, name));
                    montage.Add(bg);
                    Console.WriteLine(baseItem.GetName() + " - " + visualIdentity["DDSFile"].GetString());
                    bg.Write($@"E:\Extracted\PathOfExile\ARMOURSETS\{setIndex}_{baseItem.GetName().Replace(' ', '_')}.png");
                    setIndex++;
                    //Console.WriteLine(baseItem.GetName() + " | " + visualIdentity["DDSFile"].GetString());
                }
            }
        }
    }
    MagickImage combined = (MagickImage) montage.Montage(new MontageSettings() { BackgroundColor = MagickColors.Black, Geometry = new MagickGeometry(160, 240)});
    Console.WriteLine($"{combined.Width}x{combined.Height}");
    combined.Write(@"E:\Extracted\PathOfExile\Bodies.png");
}

//unfinished
void ListDatStringIds() {
    List<string[]> datIds = new List<string[]>();

    foreach (DatFile dat in dats.Values) {
        if (dat.RowCount < 126) continue;
        if (!dat.Spec.ContainsKey("Id") || dat.Spec["Id"].Type != ColumnType.String) continue;
        if (!dat.Name.EndsWith(".dat")) continue;

        Console.WriteLine(dat.Name);

        string[] ids = new string[128];
        ids[0] = dat.Name;

        for (int i = 9; i < 127; i++) {
            DatRow row = dat[i];
            try {
                string id = row["Id"].GetString();
                if (id is not null) ids[i] = id;
            } catch { }

        }
        datIds.Add(ids);
    }

    for (int i = 0; i < 128; i++) {
        Console.Write(i.ToString() + "|");
        for (int x = 0; x < datIds.Count; x++) Console.Write(datIds[x][i] + "|");
        Console.WriteLine();
    }


}

void ListMonsterNameLengths() {
    foreach (DatRow row in dats["MonsterVarieties.dat"]) {
        string name = row["Name"].GetString();
        Console.WriteLine($"{name.Length} {name}");
    }
}

void ListMonsterLocations() {
    var monsterLocations = BuildMonsterLocations();
    var monsterRelations = BuildMonsterRelations();
    for (int i = 0; i < dats["MonsterVarieties.dat"].RowCount; i++) {
        DatRow monsterVariety = dats["MonsterVarieties.dat"][i];

        //DatRow monsterType = monsterVariety["MonsterTypesKey"].GetReference().GetReferencedRow();
        //if (monsterType["IsSummoned"].GetPrimitive<bool>()) continue;

        string id = monsterVariety["Id"].GetString();
        string name = monsterVariety["Name"].GetString();
        if (name.Length >= 35) name = name.Substring(0, 35);
        Console.Write(id + "@" + name);

        
        if (monsterLocations.ContainsKey(i)) {
            foreach (var location in monsterLocations[i]) {
                Console.Write("@" + location[0]);
                for (int v = 1; v < location.Length; v++)
                    Console.Write(" - " + location[v]);
            }
        }
        
        if (monsterRelations.ContainsKey(i)) 
            foreach(var tuple in monsterRelations[i]) {
                if (tuple.Type != "Base" && tuple.Type != "Summoned by") continue;
                DatRow monster = dats["MonsterVarieties.dat"][tuple.Monster];
                Console.Write($"@{tuple.Type} - {GetMonsterCleanId(monster, false)} ({monster["Name"].GetString()})");
            }
        Console.WriteLine();
    }
}

void WriteArmEntityNames(int num) {
    using (TextWriter writer = new StreamWriter(File.Open($"entities{num}.txt", FileMode.Create))) {
        int i = 0;
        foreach (string path in Directory.EnumerateFiles(@"E:\Extracted\PathOfExile\3.18.Sentinel\Metadata\Terrain", "*.arm", SearchOption.AllDirectories)) {
            Arm a = new Arm(path);
            if (a.entityLines.Length <= num) continue;
            foreach (string line in a.entityLines[num]) {
                //if (line.Contains("\"\"")) continue;
                writer.WriteLine($"{a.entityLines.Length} {path.Substring(48, path.Length - 48)} {line}");
            }
            i++;
            if (i % 100 == 0) Console.WriteLine(i);
        }
    }
}

void ListAreaMonsters() {
    BuildMonsterLocations();
    foreach (string area in areaMonsters.Keys) {
        Console.WriteLine(area);
        foreach (string monster in areaMonsters[area]) {
            Console.WriteLine(monster);
        }
        Console.WriteLine();
    }
    return;
}

void ListUsedAOs() {
    HashSet<string> attatchments = new HashSet<string>();
    foreach (string ao in Directory.EnumerateFiles(@"E:\Extracted\PathOfExile\3.18.Sentinel\Metadata\Monsters", "*.ao", SearchOption.AllDirectories)) {
        foreach (string attachment in GetAttatchmentsFromAo(ao))
            attatchments.Add(@"E:\Extracted\PathOfExile\3.18.Sentinel\" + attachment.Replace('/', '\\'));
    }

    HashSet<string> aos = new HashSet<string>();
    Console.WriteLine(aos.Count);
    foreach (DatRow monster in dats["MonsterVarieties.dat"]) {
        foreach (string ao in monster["AOFiles"].GetStringArray()) {
            aos.Add(@"E:\Extracted\PathOfExile\3.18.Sentinel\" + ao.Replace('/', '\\'));
        }
    }

    foreach (string ao in Directory.EnumerateFiles(@"E:\Extracted\PathOfExile\3.18.Sentinel\Metadata\Monsters", "*.ao", SearchOption.AllDirectories)) {
        if (aos.Contains(ao)) Console.WriteLine(ao + "|MONSTER");
        else if (attatchments.Contains(ao)) Console.WriteLine(ao + "|ATTATCHMENT");
        else Console.WriteLine(ao + "|UNUSED");
    }

}



static void ListPacks() {
    Dictionary<string, List<string>> packs = new Dictionary<string, List<string>>();

    foreach (DatRow row in dats["MonsterPacks.dat"]) {
        string packName = row["Id"].GetString();
        packs[packName] = new List<string>();
        foreach (DatReference monsterRef in row["BossMonster_MonsterVarietiesKeys"].GetReferenceArray()) {
            DatRow monster = monsterRef.GetReferencedRow();
            packs[packName].Add($"{monster["Name"].GetString()}@{monster["Id"]}");
        }
    }
    foreach (DatRow row in dats["MonsterPackEntries.dat"]) {
        DatRow monster = row["MonsterVarietiesKey"].GetReference().GetReferencedRow();
        DatRow pack = row["MonsterPacksKey"].GetReference().GetReferencedRow();
        string packName = pack["Id"].GetString();
        packs[packName].Add($"{monster["Name"].GetString()}@{monster["Id"]}");
    }

    foreach (string pack in packs.Keys) {
        foreach (string monster in packs[pack]) {
            Console.WriteLine(pack + "@" + monster);
        }
        Console.WriteLine();
    }

}

    static void DumpMonsterSkills() {
        grantedEffectPerLevelsMax = BuildEffectPerLevels(dats);

        using (TextWriter writer = new StreamWriter(File.Open(@"E:\Anna\Anna\Visual Studio\Archbestiaryweb\skillstest.html", FileMode.Create))) {
            for (int i = 0; i < dats["GrantedEffects.dat"].RowCount; i++) {
                DatRow grantedEffect = dats["GrantedEffects.dat"][i];
                writer.WriteLine(CreateGrantedEffectHtml(grantedEffect, i));
            }
        }
    }

    static string ListReferenceArrayIds(DatReference[] refs, string column = "Id") {
        StringBuilder s = new StringBuilder();
        for (int i = 0; i < refs.Length; i++) s.Append(refs[i].GetReferencedRow()[column].GetString() + ", ");
        if (s.Length > 0) s.Remove(s.Length - 2, 2);
        return s.ToString();
    }


    static void ListSkillContextFlags() {
        foreach (DatRow activeSkill in dats["ActiveSkills.dat"]) {
            Console.Write(activeSkill["Id"].GetString() + ", ");
            foreach (DatReference contextFlagRef in activeSkill["ContextFlags"].GetReferenceArray()) Console.Write(contextFlagRef.GetReferencedRow()["Id"].GetString() + ", ");
            Console.WriteLine();
        }
    }

*/