using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;
using Archbestiary.Util;


Dictionary<int, DatRow> grantedEffectPerLevelsMax;

DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemaformatted.json");
DatFileIndex dats = new DatFileIndex(new DiskDirectory(@"E:\Extracted\PathOfExile\3.18.Sentinel\Data\"), spec);


//Dictionary<int, RelatedMonsters> relatedMonsters;

//foreach(string path in Directory.EnumerateFiles(@"E:\Extracted\PathOfExile\3.18.Sentinel\Metadata\Monsters", "*.ao", SearchOption.AllDirectories)) {
//    Console.WriteLine(path + " - " + GetRigFromAO(path));
//}
/*
foreach(DatRow row in dats["MonsterVarietiesArtVariations.dat"]) {
    string name = row["Id"].GetString();
    DatReference monster = row["MonsterVarieties"].GetReferenceArray()[0];
    string monsterName = monster.GetReferencedRow()["Id"].GetString();
    Console.WriteLine(name + " | " + monsterName);
}
*/


var monsterLocations = BuildMonsterLocations();
for(int i = 0; i < dats["MonsterVarieties.dat"].RowCount; i++) {
    DatRow monsterVariety = dats["MonsterVarieties.dat"][i];
    string id = monsterVariety["Id"].GetString();
    string name = monsterVariety["Name"].GetString();
    if (name.Length >= 35) name = name.Substring(0, 35);
    Console.Write(id + "@" + name);
    if (monsterLocations.ContainsKey(i))
        foreach (string[] val in monsterLocations[i]) Console.Write($"@{val[0]}@{val[1]}@{val[2]}@{val[3]}");
    Console.WriteLine();
}



//CreateMonsterPages();
//CreateMonsterList();

void CreateMonsterList() {
    StringBuilder html = new StringBuilder();
    html.AppendLine("<link rel=\"stylesheet\" href=\"index.css\"></link>");
    html.AppendLine("<body><div class=\"maindiv\">");
    html.AppendLine("<details open><summary>Regular</summary><table>");

    HashSet<int> ignore = new HashSet<int>();
    foreach (DatRow row in dats["SpectreOverrides.dat"]) {
        ignore.Add(row["Spectre"].GetReference().RowIndex);
    }

    HashSet<int> bosses = new HashSet<int>();
    foreach(DatRow row in dats["WorldAreas.dat"]) {
        foreach (DatReference boss in row["Bosses_MonsterVarietiesKeys"].GetReferenceArray()) bosses.Add(boss.RowIndex);
    }

    for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat"].RowCount; monsterVarietyRow++) {
        if (!ignore.Contains(monsterVarietyRow)) {

            DatRow monsterVariety = dats["MonsterVarieties.dat"][monsterVarietyRow];
            DatRow monsterType = monsterVariety["MonsterTypesKey"].GetReference().GetReferencedRow();
            if (monsterType["IsSummoned"].GetPrimitive<bool>()) continue;

            string monsterClass = bosses.Contains(monsterVarietyRow) ? "linkboss" : "linknormal";

            string id = monsterVariety["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
            string name = monsterVariety["Name"].GetString();
            if (name.Length >= 35) name = name.Substring(0, 35);
            html.AppendLine($"<tr><td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a></td>");
            html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{id}</a></td></tr>");
        }
    }
    html.Append("</table></details><details><summary>Summoned</summary><table>");
    for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat"].RowCount; monsterVarietyRow++) {
        if (!ignore.Contains(monsterVarietyRow)) {

            DatRow monsterVariety = dats["MonsterVarieties.dat"][monsterVarietyRow];
            DatRow monsterType = monsterVariety["MonsterTypesKey"].GetReference().GetReferencedRow();
            if (!monsterType["IsSummoned"].GetPrimitive<bool>()) continue;

            string monsterClass = bosses.Contains(monsterVarietyRow) ? "linkboss" : "linknormal";

            string id = monsterVariety["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
            string name = monsterVariety["Name"].GetString();
            html.AppendLine($"<tr><td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{id}</a></td>");
            html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a></td></tr>");
        }
    }


    html.AppendLine("</table></details></div>");
    html.AppendLine("<div class=\"mt\"><iframe name=\"body\" src=\"Monsters/AtlasInvaders_BlackStarMonsters_BlackStarBoss.html\"></iframe></div></body>");
    File.WriteAllText(@"E:\Anna\Anna\Visual Studio\Archbestiary\web\index.html", html.ToString());
}

void CreateMonsterPages() {


    var monsterLocations = BuildMonsterLocations();

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
        for (int ao = 0; ao < aos.Length; ao++) aos[ao] = aos[ao].Replace("Metadata/", "");
        for (int rig = 0; rig < rigs.Length; rig++) rigs[rig] = rigs[rig].Replace("Art/Models/", "");

        HTMLWriter html = new HTMLWriter(File.Open(@"E:\Anna\Anna\Visual Studio\Archbestiary\web\Monsters\" + monsterID.TrimEnd('_').Replace('/', '_') + ".html", FileMode.Create));
        html.WriteLine("<link rel=\"stylesheet\" href=\"monster.css\"></link>");

        html.WriteTable(
            HTML.RowClass(null, "maincolumn",
                HTML.Array(
                    HTML.TableClass("block",
                        HTML.Row($"<h4>{monsterName}</h4>"),
                        HTML.Row(monsterType["Id"].GetString())
                    ),
                    HTML.Break(),
                    HTML.TableClass("block",
                        HTML.Row("Life Mult:", lifeMult, "Ailment Threshold:", ailmentMult),
                        HTML.Row("Armour Mult:", armourMult, "Fire Resistance:", fireRes),
                        HTML.Row("Evasion Mult:", evasionMult, "Cold Resistance:", coldRes),
                        HTML.Row("Energy Shield:", esMult, "Lightning Resistance:", lightningRes),
                        HTML.Row("Damage Mult:", damageMult, "Chaos Resistance:", chaosRes)
                    ),
                    CreateGrantedEffectTables(monsterVariety)
                ),
                HTML.Array(
                    
                    HTML.TableClass("block",
                        HTML.Row("Id:", monsterID),
                        HTML.Row("Obj:", ListStrings(aos)),
                        HTML.Row("Rig:", ListStrings(rigs))
                    ),
                    HTML.Break(),
                    HTML.TableClass("block", HTML.Array(monsterLocations.ContainsKey(monsterVarietyRow) ? monsterLocations[monsterVarietyRow].ToHTMLTable() : null))
                )
            )
        );
        html.Close();

        //RELATED MONSTERS MOVE LATER
        /*
        if (spectreChildren.ContainsKey(monsterVarietyRow)) {
            string spectre = spectreChildren[monsterVarietyRow].GetReferencedRow()["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
            html.AppendLine($"<a href=\"{spectre.Replace('/', '_')}.html\" target=\"body\">Spectre: {spectre}</a>");
        } else if(spectreParents.ContainsKey(monsterVarietyRow)) {
            string parent = spectreParents[monsterVarietyRow].GetReferencedRow()["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
            html.AppendLine($"<a href=\"{parent.Replace('/', '_')}.html\" target=\"body\">Base: {parent}</a>");
        }
        */

        //GRANTEDEFFECTS MOVE LATER
        //foreach (DatReference r in ) if (r is not null) {
        //    html.AppendLine(CreateGrantedEffectHtml(r.GetReferencedRow(), r.RowIndex));
        //}




        if (monsterVarietyRow % 100 == 0) Console.WriteLine(monsterVarietyRow);
        //Console.WriteLine(monsterID);

    }
}

Dictionary<int, List<string[]>> BuildMonsterLocations() {
    Dictionary<int, List<string[]>> monsterLocations = new Dictionary<int, List<string[]>>();
    foreach (DatRow area in dats["WorldAreas.dat"]) {
        string areaName = area["Name"].GetString();
        string areaID = area["Id"].GetString();
        string act = $"Act {area["Act"].GetPrimitive<int>()}";
        foreach (DatReference monster in area["Bosses_MonsterVarietiesKeys"].GetReferenceArray())
            AddMonsterLocation(monsterLocations, monster.RowIndex, act, areaName, areaID, "Boss");
        foreach (DatReference monster in area["Monsters_MonsterVarietiesKeys"].GetReferenceArray()) 
            AddMonsterLocation(monsterLocations, monster.RowIndex, act, areaName, areaID, "Enemy");
    }
    foreach(DatRow row in dats["InvasionMonstersPerArea.dat"]) {
        DatRow area = row["WorldAreasKey"].GetReference().GetReferencedRow();
        string areaName = area["Name"].GetString();
        string areaID = area["Id"].GetString();
        string act =  $"Act {area["Act"].GetPrimitive<int>()}";
        foreach (DatReference monster in row["MonsterVarietiesKeys1"].GetReferenceArray())
            AddMonsterLocation(monsterLocations, monster.RowIndex, act, areaName, areaID, "Invasion 1");
        foreach (DatReference monster in row["MonsterVarietiesKeys2"].GetReferenceArray())
            AddMonsterLocation(monsterLocations, monster.RowIndex, act, areaName, areaID, "Invasion 2");
    }

    return monsterLocations;
}

void AddMonsterLocation(Dictionary<int, List<string[]>> monsterLocations, int monster, params string[] values) {
    if (!monsterLocations.ContainsKey(monster)) monsterLocations[monster] = new List<string[]>();
    monsterLocations[monster].Add(values);
}

string CreateGrantedEffectTables(DatRow monsterVariety) {
    DatReference[] refs = monsterVariety["GrantedEffectsKeys"].GetReferenceArray();
    if (refs is null) return "";
    StringBuilder effectTables = new StringBuilder();
    for (int i = 0; i < refs.Length; i++) {
        effectTables.AppendLine(CreateGrantedEffectHtml(refs[i].GetReferencedRow(), refs[i].RowIndex));
    }
    return effectTables.ToString();
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

    html.AppendLine("<br/><table class=\"block\">");
    html.AppendLine($"<tr><td><h4>{grantedEffectName} ({row})</h4></td></tr>");
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
    html.Append("</table>");
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
                return $"Summons <a href=\"{cleanId}.html\" target=\"body\">{monsterVariety["Name"].GetString()}</a>";
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

void ListMonsterNameLengths() {
    foreach (DatRow row in dats["MonsterVarieties.dat"]) {
        string name = row["Name"].GetString();
        Console.WriteLine($"{name.Length} {name}");
    }
}