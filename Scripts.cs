using ImageMagick;
using PoeSharp.Filetypes.Dat;

static class Scripts {
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

    public static void MonsterLife(Bestiary b) {
        Console.Write("    const lifePerLevel = [");
        for(int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row["Life"].GetPrimitive<int>().ToString() + ", ");
        }
        Console.Write("];\n    const armourPerLevel = [");
        for (int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row["Armour"].GetPrimitive<int>().ToString() + ", ");
        }
        Console.Write("];\n    const evasionPerLevel = [");
        for (int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row["Evasion"].GetPrimitive<int>().ToString() + ", ");
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

void ListMonsterRigs() {
    for(int i = 0; i < dats["MonsterVarieties.dat"].RowCount; i++) {
        DatRow monster = dats["MonsterVarieties.dat"][i];
        DatReference monsterType = monster["MonsterTypesKey"].GetReference();
        string monsterTypeId = monsterType.GetReferencedRow()["Id"].GetString();
        string monsterId = monster["Id"].GetString();

        foreach (string ao in monster["AOFiles"].GetStringArray()) {
            string rig = GetRigFromAO(Path.Combine(@"E:\Extracted\PathOfExile\3.18.Sentinel", ao));
            Console.WriteLine($"{monsterType.RowIndex}@{monsterTypeId}@{i}@{monsterId}@{ao}@{rig}");
        }
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

    static void ListDatRowCounts() {
        foreach (DatFile dat in dats.Values) {
            if (dat.Name.EndsWith(".dat"))
                Console.WriteLine($"{dat.RowCount}|{dat.Name}");
        }
    }

    static void ListSkillContextFlags() {
        foreach (DatRow activeSkill in dats["ActiveSkills.dat"]) {
            Console.Write(activeSkill["Id"].GetString() + ", ");
            foreach (DatReference contextFlagRef in activeSkill["ContextFlags"].GetReferenceArray()) Console.Write(contextFlagRef.GetReferencedRow()["Id"].GetString() + ", ");
            Console.WriteLine();
        }
    }

*/