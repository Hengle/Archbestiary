using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;



Dictionary<int, DatRow> grantedEffectPerLevelsMax;

DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemaformatted.json");
DatFileIndex dats = new DatFileIndex(new DiskDirectory(@"E:\Extracted\PathOfExile\3.18.Sentinel\Data\"), spec);


//Dictionary<int, RelatedMonsters> relatedMonsters;

//foreach(string path in Directory.EnumerateFiles(@"E:\Extracted\PathOfExile\3.18.Sentinel\Metadata\Monsters", "*.ao", SearchOption.AllDirectories)) {
//    Console.WriteLine(path + " - " + GetRigFromAO(path));
//}


CreateMonsterPages();
CreateMonsterList();

void CreateMonsterList() {
    StringBuilder html = new StringBuilder();

    html.AppendLine("<body style=\"margin: 0px;\"><div style=\"width:50%; position:fixed; overflow-x:hidden; overflow-y:scroll; height:100%; display:grid;\"><table>");

    HashSet<int> ignore = new HashSet<int>();
    foreach (DatRow row in dats["SpectreOverrides.dat"]) {
        ignore.Add(row["Spectre"].GetReference().RowIndex);
    }

    for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat"].RowCount; monsterVarietyRow++) {
        if (!ignore.Contains(monsterVarietyRow)) {

            DatRow monsterVariety = dats["MonsterVarieties.dat"][monsterVarietyRow];
            string monsterType = monsterVariety["MonsterTypesKey"].GetReference().GetReferencedRow()["Id"].GetString();
            if (monsterType == "Daemon") continue;

            string id = monsterVariety["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
            string name = monsterVariety["Name"].GetString();
            html.AppendLine($"<tr><td style=\"white-space:nowrap\"><a href=\"Monsters/{id.Replace('/', '_')}.html\" target=\"body\">{id}</a></td>");
            html.AppendLine($"<td style=\"white-space:nowrap\"><a href=\"Monsters/{id.Replace('/', '_')}.html\" target=\"body\">{name}</a></td></tr>");
        }
    }

    html.AppendLine("</table></div><div class=\"mt\" style=\"margin-left:50%;\"><iframe name=\"body\" src=\"Monsters/AtlasInvaders_BlackStarMonsters_BlackStarBoss.html\" height=\"100%\" width=\"100%\" style=\"border: 0px; background-color: #eeeeee;\"></iframe></div></body>");
    File.WriteAllText(@"E:\Anna\Anna\Visual Studio\Archbestiaryweb\index.html", html.ToString());
}

void CreateMonsterPages() {


    //SPECTRE OVERRIDES
    Dictionary<int, DatReference> spectreParents = new Dictionary<int, DatReference>();
    Dictionary<int, DatReference> spectreChildren = new Dictionary<int, DatReference>();

    foreach (DatRow row in dats["SpectreOverrides.dat"]) {
        DatReference parent = row["Monster"].GetReference();
        DatReference child = row["Spectre"].GetReference();
        spectreParents[child.RowIndex] = parent;
        spectreChildren[parent.RowIndex] = child;
    }

    //SUMMONS
    Dictionary<int, DatReference> summonMonsters = new Dictionary<int, DatReference>();
    foreach(DatRow row in dats["SummonedSpecificMonsters.dat"]) {
        DatReference r = row["MonsterVarietiesKey"].GetReference();
        if (r is not null) summonMonsters[row["Id"].GetPrimitive<int>()] = r;
    }

    grantedEffectPerLevelsMax = BuildEffectPerLevels(dats);


    for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat"].RowCount; monsterVarietyRow++) {
        var monsterVariety = dats["MonsterVarieties.dat"][monsterVarietyRow];
        DatReference monsterTypeRef = monsterVariety["MonsterTypesKey"].GetReference();
        DatRow monsterType = monsterTypeRef.GetReferencedRow();

        int fireRes = 0; int coldRes = 0; int lightningRes = 0; int chaosRes = 0;
        DatReference? resReference = monsterType["MonsterResistancesKey"].GetReference();
        if (resReference is not null) {
            DatRow monsterResistance = resReference.GetReferencedRow();
            fireRes = monsterResistance["FireMerciless"].GetPrimitive<int>();
            coldRes = monsterResistance["ColdMerciless"].GetPrimitive<int>();
            lightningRes = monsterResistance["LightningMerciless"].GetPrimitive<int>();
            chaosRes = monsterResistance["ChaosMerciless"].GetPrimitive<int>();
        }

        int lifeMult = monsterVariety["LifeMultiplier"].GetPrimitive<int>();
        int ailmentMult = monsterVariety["AilmentThresholdMultiplier"].GetPrimitive<int>();
        int armourMult = monsterType["Armour"].GetPrimitive<int>();
        int evasionMult = monsterType["Evasion"].GetPrimitive<int>();
        int esMult = monsterType["EnergyShieldFromLife"].GetPrimitive<int>();

        int damageMult = monsterVariety["DamageMultiplier"].GetPrimitive<int>();
        int attackTime = monsterVariety["AttackSpeed"].GetPrimitive<int>();



        string monsterID = monsterVariety["Id"].GetString();
        monsterID = monsterID.Replace("Metadata/Monsters/", "");
        string monsterName = monsterVariety["Name"].GetString();

        //Console.WriteLine($"{monsterID}|{monsterName}|{monsterVariety["TEST"].GetPrimitive<bool>()}");
        //continue;

        //Console.WriteLine($"{i}\t{monsterID}\t{monsterName}\t{monsterTypeRef.RowIndex}\t{monsterType["Id"].GetString()}");
        //continue;

        //Console.WriteLine(MakeLine( monsterType["Id"].GetString(), 
        //    lifeMult, ailmentMult, armourMult, evasionMult, esMult, fireRes, coldRes, lightningRes, chaosRes));

        //<tr><td colspan=""4"">{ListReferenceArrayIds(monsterVariety["TagsKeys"].GetReferenceArray())}</td></tr>

        string[] aos = monsterVariety["AOFiles"].GetStringArray();
        string[] rigs = new string[aos.Length]; for (int ao = 0; ao < aos.Length; ao++) rigs[ao] = GetRigFromAO(@"E:\Extracted\PathOfExile\3.18.Sentinel\" + aos[ao]);


        StringBuilder html = new StringBuilder();

        if(spectreChildren.ContainsKey(monsterVarietyRow)) {
            string spectre = spectreChildren[monsterVarietyRow].GetReferencedRow()["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
            html.AppendLine($"<a href=\"{spectre.Replace('/', '_')}.html\" target=\"body\">Spectre: {spectre}</a>");
        } else if(spectreParents.ContainsKey(monsterVarietyRow)) {
            string parent = spectreParents[monsterVarietyRow].GetReferencedRow()["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
            html.AppendLine($"<a href=\"{parent.Replace('/', '_')}.html\" target=\"body\">Base: {parent}</a>");
        }

        html.AppendLine($@"
    <table>
        <tr><td colspan=""4""><h4 style=""margin-bottom: 0px;"">{monsterName}</h4></td></tr>
        <tr><td colspan=""4"">{monsterID}</td></tr>
        <tr><td colspan=""4"">{monsterType["Id"].GetString()}</td></tr>
        <tr><td colspan=""4"">{ListStrings(aos)}</td></tr>
        <tr><td colspan=""4"">{ListStrings(rigs)}</td></tr>
        <tr><td>Life Mult:</td><td>{lifeMult}</td><td>Ailment Threshold:</td><td>{ailmentMult}</td></tr>
        <tr><td>Armour Mult:</td><td>{armourMult}</td><td>Fire Resistance:</td><td>{fireRes}</td></tr>
        <tr><td>Evasion Mult:</td><td>{evasionMult}</td><td>Cold Resistance:</td><td>{coldRes}</td></tr>
        <tr><td>Energy Shield Mult:</td><td>{esMult}</td><td>Lightning Resistance:</td><td>{lightningRes}</td></tr>
        <tr><td>Damage Mult:</td><td>{damageMult}</td><td>Chaos Resistance:</td><td>{chaosRes}</td></tr>
    </table>
    ");

        foreach (DatReference r in monsterVariety["GrantedEffectsKeys"].GetReferenceArray()) if (r is not null) {
            html.AppendLine(CreateGrantedEffectHtml(r.GetReferencedRow(), r.RowIndex));
         }



        File.WriteAllText(@"E:\Anna\Anna\Visual Studio\Archbestiaryweb\Monsters\" + monsterID.TrimEnd('_').Replace('/', '_') + ".html", html.ToString());

        if (monsterVarietyRow % 100 == 0) Console.WriteLine(monsterVarietyRow);
        //Console.WriteLine(monsterID);

    }
}

string CreateGrantedEffectHtml(DatRow grantedEffect, int row) {
    StringBuilder html = new StringBuilder();
    DatReference rSkill = grantedEffect["ActiveSkill"].GetReference();
    if (rSkill is null) {
        Console.WriteLine(grantedEffect["Id"].GetString() + " Has no active skill");
        return "";
    }
    DatRow activeSkill = rSkill.GetReferencedRow();
    string grantedEffectName = grantedEffect["Id"].GetString(); string skillName = activeSkill["Id"].GetString();
    DatRow grantedEffectPerLevel = grantedEffectPerLevelsMax[row];

    html.AppendLine("<br/><table>");
    html.AppendLine($"<tr><td><h4 style=\"margin-bottom: 0px;\">{grantedEffectName} ({row})</h4></td></tr>");
    string damageType = GetSkillDamageTypes(activeSkill);
    if (damageType is not null)
        html.AppendLine($"<tr><td>{skillName} ({rSkill.RowIndex}) - {damageType}</td></tr>");
    else
        html.AppendLine($"<tr><td>{skillName} ({rSkill.RowIndex})</td></tr>");


    float[] floatStatValues = new float[] {
                    grantedEffectPerLevel["Stat1Float"].GetPrimitive<float>(),
                    grantedEffectPerLevel["Stat2Float"].GetPrimitive<float>(),
                    grantedEffectPerLevel["Stat3Float"].GetPrimitive<float>(),
                    grantedEffectPerLevel["Stat4Float"].GetPrimitive<float>(),
                    grantedEffectPerLevel["Stat5Float"].GetPrimitive<float>(),
                    grantedEffectPerLevel["Stat6Float"].GetPrimitive<float>(),
                    grantedEffectPerLevel["Stat7Float"].GetPrimitive<float>(),
                    grantedEffectPerLevel["Stat8Float"].GetPrimitive<float>(),
                    grantedEffectPerLevel["Stat9Float"].GetPrimitive<float>()
                };

    int[] intStatValues = new int[] {
                    grantedEffectPerLevel["Stat1Value"].GetPrimitive<int>(),
                    grantedEffectPerLevel["Stat2Value"].GetPrimitive<int>(),
                    grantedEffectPerLevel["Stat3Value"].GetPrimitive<int>(),
                    grantedEffectPerLevel["Stat4Value"].GetPrimitive<int>(),
                    grantedEffectPerLevel["Stat5Value"].GetPrimitive<int>(),
                    grantedEffectPerLevel["Stat6Value"].GetPrimitive<int>(),
                    grantedEffectPerLevel["Stat7Value"].GetPrimitive<int>(),
                    grantedEffectPerLevel["Stat8Value"].GetPrimitive<int>(),
                    grantedEffectPerLevel["Stat9Value"].GetPrimitive<int>()
                };

    var grantedEffectStats = grantedEffectPerLevel["StatsKeys"].GetReferenceArray();
    for (int gei = 0; gei < grantedEffectStats.Length; gei++) {
        html.AppendLine($"<tr><td>{GetStatDescription(grantedEffectStats[gei].GetReferencedRow(), intStatValues[gei], floatStatValues[gei])}</td></tr>");
    }

    foreach (DatReference staticStatRef in grantedEffectPerLevel["StatsKeys2"].GetReferenceArray()) {
        html.AppendLine($"<tr><td>{staticStatRef.GetReferencedRow()["Id"].GetString()}</td></tr>");
    }
    html.AppendLine("</table><br/>");
    return html.ToString();
}

void DumpMonsterSkills() {
    grantedEffectPerLevelsMax = BuildEffectPerLevels(dats);

    using (TextWriter writer = new StreamWriter(File.Open(@"E:\Anna\Anna\Visual Studio\Archbestiaryweb\skillstest.html", FileMode.Create))) {
        for (int i = 0; i < dats["GrantedEffects.dat"].RowCount; i++) {
            DatRow grantedEffect = dats["GrantedEffects.dat"][i];
            writer.WriteLine(CreateGrantedEffectHtml(grantedEffect, i));
        }
    }
}

string GetStatDescription(DatRow stat, int intStatValue, float floatStatValue) {
    string id = stat["Id"].GetString();
    if(id == "alternate_minion") {
        for(int summonRow = 0; summonRow < dats["SummonedSpecificMonsters.dat"].RowCount; summonRow++) {
            DatRow row = dats["SummonedSpecificMonsters.dat"][summonRow];
            int summonId = row["Id"].GetPrimitive<int>();

            if (summonId == intStatValue) {
                DatReference monsterRef = row["MonsterVarietiesKey"].GetReference();
                if (monsterRef is null) return $"Summons UNKNOWN {intStatValue}";
                DatRow monsterVariety = monsterRef.GetReferencedRow();
                string cleanId = GetMonsterCleanId(monsterVariety);
                return $"Summons <a href=\"{cleanId}.html\" target=\"body\">{monsterVariety["Name"].GetString()} ({cleanId})</a>";
            }
        }
    }
    return $"{id} {intStatValue} {floatStatValue}";
}

string GetMonsterCleanId(DatRow monsterVariety) {
    return monsterVariety["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_').Replace('/', '_');
}


string GetRigFromAO(string path) {
    //super hacky
    foreach(string line in File.ReadAllLines(path)) {
        if(line.Contains("metadata =")) {
            return line.Substring(line.IndexOf('"')).Trim('"');
        }
    }
    return "COULD NOT FIND RIG";
}

string ListReferenceArrayIds(DatReference[] refs, string column = "Id") {
    StringBuilder s = new StringBuilder();
    for (int i = 0; i < refs.Length; i++) s.Append(refs[i].GetReferencedRow()[column].GetString() + ", ");
    if(s.Length > 0) s.Remove(s.Length - 2, 2);
    return s.ToString();
}

string GetSkillDamageTypes(DatRow activeSkill) {
    HashSet<int> contextFlags = new HashSet<int>();
    foreach (DatReference contextFlagRef in activeSkill["ContextFlags"].GetReferenceArray()) contextFlags.Add(contextFlagRef.RowIndex);
    StringBuilder s = new StringBuilder();

    if (contextFlags.Contains(2)) s.Append("Attack/");
    if (contextFlags.Contains(3)) s.Append("Spell/");
    if (contextFlags.Contains(4)) s.Append("Secondary/");
    if (!contextFlags.Contains(18) && contextFlags.Contains(16)) {
        if (contextFlags.Contains(12)) s.Append("Spell Damage over Time/");
        else s.Append("Damage over Time/");
    }
    if (s.Length > 0) { s.Remove(s.Length - 1, 1); return s.ToString(); }
    return null;
}


void ListDatRowCounts() {
    foreach (DatFile dat in dats.Values) {
        if (dat.Name.EndsWith(".dat"))
            Console.WriteLine($"{dat.RowCount}|{dat.Name}");
    }
}

string ListStrings(params object[] vals) {
    StringBuilder s = new StringBuilder();
    for (int i = 0; i < vals.Length; i++) { s.Append(vals[i].ToString()); s.Append('\t'); }
    return s.ToString();
}

void ListSkillContextFlags() {
    foreach (DatRow activeSkill in dats["ActiveSkills.dat"]) {
        Console.Write(activeSkill["Id"].GetString() + ", ");
        foreach (DatReference contextFlagRef in activeSkill["ContextFlags"].GetReferenceArray()) Console.Write(contextFlagRef.GetReferencedRow()["Id"].GetString() + ", ");
        Console.WriteLine();
    }
}

Dictionary<int, DatRow> BuildEffectPerLevels(DatFileIndex dats) {
    Dictionary<int, DatRow> effectPerLevels = new Dictionary<int, DatRow>();

    foreach(DatRow row in dats["GrantedEffectsPerLevel.dat"]) {
        int grantedEffect = row["GrantedEffectsKey"].GetReference().RowIndex;
        if (!effectPerLevels.ContainsKey(grantedEffect)) effectPerLevels[grantedEffect] = row;
        else {
            if (row["Level"].GetPrimitive<int>() > effectPerLevels[grantedEffect]["Level"].GetPrimitive<int>()) effectPerLevels[grantedEffect] = row;
        }
    }

    return effectPerLevels;
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



class ChildMonsters {
    DatReference spectre;
    List<DatReference> summons;
}