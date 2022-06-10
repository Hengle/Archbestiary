using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;
using Archbestiary.Util;
using PoeTerrain;
using ImageMagick;




 
Dictionary<int, DatRow> grantedEffectPerLevelsMax;
Dictionary<string, HashSet<string>> areaMonsters = new Dictionary<string, HashSet<string>>();
DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemaformatted.txt");
DatFileIndex dats = new DatFileIndex(new DiskDirectory(@"E:\Extracted\PathOfExile\3.18.Sentinel\Data\"), spec);


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

//ListPacks();
//return;

//CreateMonsterPages();
CreateMonsterList();

void ListPacks() {
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

    foreach(string pack in packs.Keys) {
        foreach(string monster in packs[pack]) {
            Console.WriteLine(pack + "@" + monster);
        }
        Console.WriteLine();
    }

}

void CreateMonsterList() {

    Dictionary<string, string> monsterAdded = History.BuildMonsterVarietyHistory(true);

    StringBuilder html = new StringBuilder();
    html.AppendLine("<link rel=\"stylesheet\" href=\"index.css\"></link>");
    html.AppendLine("<body><div class=\"maindiv\">");
    html.AppendLine("<details open><summary>Regular</summary><table>");

    HashSet<int> ignore = new HashSet<int>();

    foreach (DatRow row in dats["SpectreOverrides.dat"]) 
        ignore.Add(row["Spectre"].GetReference().RowIndex);
    
    foreach (DatRow row in dats["ElderMapBossOverride.dat"])
        foreach (DatReference bossRef in row["MonsterVarietiesKeys"].GetReferenceArray())
            ignore.Add(bossRef.RowIndex);
        

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

            string id = monsterVariety["Id"].GetString().TrimEnd('_');
            string added = monsterAdded.ContainsKey(id) ? monsterAdded[id] : "U";
            id = id.Replace("Metadata/Monsters/", "");
            
            string name = monsterVariety["Name"].GetString();
            if (name.Length >= 35) name = name.Substring(0, 35);
            html.AppendLine($"<tr><td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a></td>");
            html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{added}</a></td>");
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



            string id = monsterVariety["Id"].GetString().TrimEnd('_');
            string added = monsterAdded.ContainsKey(id) ? monsterAdded[id] : "U";
            id = id.Replace("Metadata/Monsters/", "");

            string name = monsterVariety["Name"].GetString();
            if (name.Length >= 35) name = name.Substring(0, 35);
            html.AppendLine($"<tr><td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a></td>");
            html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{added}</a></td>");
            html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{id}</a></td></tr>");
        }
    }


    html.AppendLine("</table></details></div>");
    html.AppendLine("<div class=\"mt\"><iframe name=\"body\" src=\"Monsters/AtlasInvaders_BlackStarMonsters_BlackStarBoss.html\"></iframe></div></body>");
    File.WriteAllText(@"E:\Anna\Anna\Visual Studio\Archbestiary\web\index.html", html.ToString());
}

void CreateMonsterPages() {


    var monsterLocations = BuildMonsterLocations();
    var monsterRelations = BuildMonsterRelations();

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
                    CreateMonsterRelationTable(monsterRelations, monsterVarietyRow),
                    HTML.Break(),
                    HTML.TableClass("block", HTML.Array(monsterLocations.ContainsKey(monsterVarietyRow) ? monsterLocations[monsterVarietyRow].ToHTMLTableFixedColumns(4) : null))
                )
            )
        );
        html.Close();


        if (monsterVarietyRow % 100 == 0) Console.WriteLine(monsterVarietyRow);
    }
}

Dictionary<int, List<string[]>> BuildMonsterLocations() {
    Dictionary<int, List<string[]>> monsterLocations = new Dictionary<int, List<string[]>>();
    foreach (DatRow area in dats["WorldAreas.dat"]) {
        foreach (DatReference monster in area["Bosses_MonsterVarietiesKeys"].GetReferenceArray())
            AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Boss");
        foreach (DatReference monster in area["Monsters_MonsterVarietiesKeys"].GetReferenceArray()) 
            AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Enemy");
    }
    foreach(DatRow row in dats["InvasionMonstersPerArea.dat"]) {
        DatRow area = row["WorldAreasKey"].GetReference().GetReferencedRow();
        foreach (DatReference monster in row["MonsterVarietiesKeys1"].GetReferenceArray())
            AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Invasion 1");
        foreach (DatReference monster in row["MonsterVarietiesKeys2"].GetReferenceArray())
            AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Invasion 2");
    }


    foreach(DatRow row in dats["MonsterPackEntries.dat"]) {
        int monster = row["MonsterVarietiesKey"].GetReference().RowIndex;
        DatRow pack = row["MonsterPacksKey"].GetReference().GetReferencedRow();
        foreach(DatReference areaRef in pack["WorldAreasKeys"].GetReferenceArray()) {
            DatRow area = areaRef.GetReferencedRow();
            if (!AreaIsExtraSynthesis(area)) {
                string packName = pack["Id"].GetString();
                AddMonsterLocation2(monsterLocations, monster, area, "Pack", packName);
            }
        }
    }

    foreach(DatRow row in dats["MonsterPacks.dat"]) {
        foreach (DatReference areaRef in row["WorldAreasKeys"].GetReferenceArray()) {
            DatRow area = areaRef.GetReferencedRow();
            if (!AreaIsExtraSynthesis(area)) {
                string packName = row["Id"].GetString();
                foreach (DatReference monster in row["BossMonster_MonsterVarietiesKeys"].GetReferenceArray()) 
                    AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Pack Boss", packName);
            }
        }
    }

    Dictionary<int, HashSet<string>> monsterSpawners = new Dictionary<int, HashSet<string>>();
    foreach(DatRow row in dats["TableMonsterSpawners.dat"]) {
        string id = row["Metadata"].GetString().Replace("Metadata/", "");
        foreach(DatReference monsterRef in row["MonsterVarieties"].GetReferenceArray()) {
            if (!monsterSpawners.ContainsKey(monsterRef.RowIndex)) monsterSpawners[monsterRef.RowIndex] = new HashSet<string>();
            monsterSpawners[monsterRef.RowIndex].Add(id);
        }
    }
    foreach(int monster in monsterSpawners.Keys)
        foreach (string spawner in monsterSpawners[monster])
            AddMonsterLocation(monsterLocations, monster, "Spawner", spawner);


    return monsterLocations;
}

bool AreaIsExtraSynthesis(DatRow area) {
    string id = area["Id"].GetString();
    if (id.StartsWith("Synthesis_Single") && id.Length > "Synthesis_Single".Length) return true;
    else if (id.StartsWith("Synthesis_Main") && id.Length > "Synthesis_Main".Length) return true;
    return false;
}

void AddMonsterLocation(Dictionary<int, List<string[]>> monsterLocations, int monster, params string[] values) {
    if (!monsterLocations.ContainsKey(monster)) monsterLocations[monster] = new List<string[]>();
    monsterLocations[monster].Add(values);
}

void AddMonsterLocation2(Dictionary<int, List<string[]>> monsterLocations, int monster, DatRow area, string type, string idReplace = null) {

    //TEMP
    string areaDesc = $"{area["Name"].GetString()}@@{area["Id"].GetString()}";
    DatRow monsterRow = dats["MonsterVarieties.dat"][monster];
    string monsterDesc = $"{monsterRow["Name"].GetString()}@{type}@{monsterRow["Id"].GetString()}";
    if (!areaMonsters.ContainsKey(areaDesc)) areaMonsters[areaDesc] = new HashSet<string>();
    areaMonsters[areaDesc].Add(monsterDesc);

    string act = $"Act {area["Act"].GetPrimitive<int>()}";
    string areaName = area["Name"].GetString();
    string areaID = idReplace is null ? area["Id"].GetString() : idReplace;
    if (!monsterLocations.ContainsKey(monster)) monsterLocations[monster] = new List<string[]>();
    if (areaName == "NULL")  monsterLocations[monster].Add(new string[] { type, areaID });
    else monsterLocations[monster].Add(new string[] {type, act, areaName, areaID});
}



Dictionary<int, HashSet<(int Monster, string Type)>> BuildMonsterRelations() {
    var monsterRelations = new Dictionary<int, HashSet<(int Monster, string Type)>>();
    foreach (DatRow row in dats["SpectreOverrides.dat"]) {
        AddMonsterRelation(monsterRelations, row["Monster"].GetReference().RowIndex, row["Spectre"].GetReference().RowIndex, "Base", "Spectre");
    }

    //Works bad for maps with multiple bosses
    foreach(DatRow row in dats["ElderMapBossOverride.dat"]) {
        DatRow area = row["WorldAreasKey"].GetReference().GetReferencedRow();
        foreach(DatReference bossRef in area["Bosses_MonsterVarietiesKeys"].GetReferenceArray()) {
            foreach(DatReference replacementRef in row["MonsterVarietiesKeys"].GetReferenceArray()) {
                AddMonsterRelation(monsterRelations, bossRef.RowIndex, replacementRef.RowIndex, "Base", "Elder Boss Dummy");
            }
        }
    }

    //summons (god this is annoying)
    Dictionary<int, int> summonedSpecificMonstersIds = new Dictionary<int, int>();
    foreach(DatRow row in dats["SummonedSpecificMonsters.dat"]) {
        DatReference monsterRef = row["MonsterVarietiesKey"].GetReference();
        if (monsterRef is null) summonedSpecificMonstersIds[row["Id"].GetPrimitive<int>()] = 0;
        else summonedSpecificMonstersIds[row["Id"].GetPrimitive<int>()] = monsterRef.RowIndex;
    }

    var perLevels = BuildEffectPerLevels(dats);
    for (int varietyRow = 0; varietyRow < dats["MonsterVarieties.dat"].RowCount; varietyRow++) {
        DatRow variety = dats["MonsterVarieties.dat"][varietyRow];
        foreach(DatReference effectRef in variety["GrantedEffectsKeys"].GetReferenceArray()) {
            DatRow perLevel = perLevels[effectRef.RowIndex];
            int[] intStatValues = new int[] {
                    perLevel["Stat1Value"].GetPrimitive<int>(),
                    perLevel["Stat2Value"].GetPrimitive<int>(),
                    perLevel["Stat3Value"].GetPrimitive<int>(),
                    perLevel["Stat4Value"].GetPrimitive<int>(),
                    perLevel["Stat5Value"].GetPrimitive<int>(),
                    perLevel["Stat6Value"].GetPrimitive<int>(),
                    perLevel["Stat7Value"].GetPrimitive<int>(),
                    perLevel["Stat8Value"].GetPrimitive<int>(),
                    perLevel["Stat9Value"].GetPrimitive<int>()
             };
            var statRefs = perLevel["StatsKeys"].GetReferenceArray();
            for(int iStat = 0; iStat < statRefs.Length; iStat++) {
                if (statRefs[iStat].GetReferencedRow()["Id"] == "alternate_minion") {
                    AddMonsterRelation(monsterRelations, varietyRow, summonedSpecificMonstersIds[intStatValues[iStat]], "Summoned by", "Summons");
                }
            }

        }
    }

    return monsterRelations;
}

void AddMonsterRelation(Dictionary<int, HashSet<(int Monster, string Type)>> monsterRelations, int parentMonster, int childMonster, string parentType, string childType) {
    if (!monsterRelations.ContainsKey(parentMonster)) monsterRelations[parentMonster] = new HashSet<(int Monster, string Type)>();
    monsterRelations[parentMonster].Add((childMonster, childType));
    if (!monsterRelations.ContainsKey(childMonster)) monsterRelations[childMonster] = new HashSet<(int Monster, string Type)>();
    monsterRelations[childMonster].Add((parentMonster, parentType));
}

string CreateMonsterRelationTable(Dictionary<int, HashSet<(int Monster, string Type)>> monsterRelations, int monster) {
    if (!monsterRelations.ContainsKey(monster)) return null;
    List<string> relations = new List<string>();
    foreach (var tuple in monsterRelations[monster]) {
        DatRow other = dats["MonsterVarieties.dat"][tuple.Monster];
        string link = GetMonsterCleanId(other) + ".html";
        string name = HTML.Link(link, other["Name"].GetString());
        string id = HTML.Link(link, GetMonsterCleanId(other, false));
        relations.Add(HTML.Row(tuple.Type + ':', name, id));
    }
    return HTML.TableClass("block", relations.ToArray());
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

string GetMonsterCleanId(DatRow monsterVariety, bool replaceSlashes = true) {
    if(replaceSlashes) return monsterVariety["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_').Replace('/', '_');
    return monsterVariety["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
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

string[] GetAttatchmentsFromAo(string path) {
    //super hacky
    List<string> attatchments = new List<string>();
    foreach (string line in File.ReadAllLines(path)) {
        if (line.Contains("attached_object =")) {
            attatchments.Add(line.Substring(line.LastIndexOf(' ') + 1).Trim('"'));
        }
    }
    return attatchments.ToArray();
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