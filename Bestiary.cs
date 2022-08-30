using System;
using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;
using Archbestiary.Util;
using PoeTerrain;

public class Bestiary {
    Dictionary<int, DatRow> grantedEffectPerLevelsMax;
    Dictionary<int, DatRow> grantedStatSetPerLevelsMax;
    Dictionary<string, HashSet<string>> areaMonsters = new Dictionary<string, HashSet<string>>();
    //DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemaformatted.json");
    //DatFileIndex dats = new DatFileIndex(new DiskDirectory(@"E:\Extracted\PathOfExile\3.18.Sentinel\Data\"), spec);
    DatSpecIndex spec;
    public DatFileIndex dats;



    //ListPacks();
    //return;

    public Bestiary() {
        spec = DatSpecIndex.Create(@"E:\Anna\Downloads\schema.min(3).json");
        dats = new DatFileIndex(new DiskDirectory(@"F:\Extracted\PathOfExile\3.19.Kalandra\Data\"), spec);
    }

    //CreateMonsterList();

    public void CreateMonsterList() {

        //Dictionary<string, string> monsterAdded = History.BuildMonsterVarietyHistory(true);
        Dictionary<string, string> monsterAdded = new Dictionary<string, string>();

        StringBuilder html = new StringBuilder();
        html.AppendLine("<link rel=\"stylesheet\" href=\"index.css\"></link>");
        html.AppendLine("<body><div class=\"maindiv\">");
        html.AppendLine("<details open><summary>Regular</summary><table>");

        HashSet<int> ignore = new HashSet<int>();

        foreach (DatRow row in dats["SpectreOverrides.dat64"])
            ignore.Add(row["Spectre"].GetReference().RowIndex);

        foreach (DatRow row in dats["ElderMapBossOverride.dat64"])
            foreach (DatReference bossRef in row["MonsterVarietiesKeys"].GetReferenceArray())
                ignore.Add(bossRef.RowIndex);
            

        HashSet<int> bosses = new HashSet<int>();
        foreach (DatRow row in dats["WorldAreas.dat64"]) {
            foreach (DatReference boss in row["Bosses_MonsterVarietiesKeys"].GetReferenceArray()) {
                bosses.Add(boss.RowIndex);
            }

        }
        Console.WriteLine(bosses.Count);

        for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat64"].RowCount; monsterVarietyRow++) {
            if (!ignore.Contains(monsterVarietyRow)) {

                DatRow monsterVariety = dats["MonsterVarieties.dat64"][monsterVarietyRow];
                DatRow monsterType = monsterVariety["MonsterTypesKey"].GetReference().GetReferencedRow();
                if (monsterType["IsSummoned"].GetPrimitive<bool>()) continue;

                string monsterClass = bosses.Contains(monsterVarietyRow) ? "linkboss" : "linknormal";

                string id = monsterVariety["Id"].GetString().TrimEnd('_');
                string added = monsterAdded.ContainsKey(id) ? monsterAdded[id] : "U";
                id = id.Replace("Metadata/Monsters/", "");

                int lifeMult = monsterVariety["LifeMultiplier"].GetPrimitive<int>();
                int damageMult = monsterVariety["DamageMultiplier"].GetPrimitive<int>();

                string name = monsterVariety["Name"].GetString();
                if (name.Length >= 35) name = name.Substring(0, 35);
                html.AppendLine($"<tr><td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9,lifeMult/100)}\" target=\"body\">{lifeMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9,(damageMult-50)/50)}\" target=\"body\">{damageMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{added}</a></td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{id}</a></td></tr>");
            }
        }
        html.Append("</table></details><details><summary>Summoned</summary><table>");
        for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat64"].RowCount; monsterVarietyRow++) {
            if (!ignore.Contains(monsterVarietyRow)) {

                DatRow monsterVariety = dats["MonsterVarieties.dat64"][monsterVarietyRow];
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

    public void CreateMonsterPages() {


        var monsterLocations = BuildMonsterLocations();
        var monsterRelations = BuildMonsterRelations();

        //SPECTRE OVERRIDES
        Dictionary<int, DatReference> spectreParents = new Dictionary<int, DatReference>();
        Dictionary<int, DatReference> spectreChildren = new Dictionary<int, DatReference>();

        foreach (DatRow row in dats["SpectreOverrides.dat64"]) {
            DatReference parent = row["Monster"].GetReference();
            DatReference child = row["Spectre"].GetReference();
            spectreParents[child.RowIndex] = parent;
            spectreChildren[parent.RowIndex] = child;
        }

        //SUMMONS
        Dictionary<int, DatReference> summonMonsters = new Dictionary<int, DatReference>();
        foreach (DatRow row in dats["SummonedSpecificMonsters.dat64"]) {
            DatReference r = row["MonsterVarietiesKey"].GetReference();
            if (r is not null) summonMonsters[row["Id"].GetPrimitive<int>()] = r;
        }

        grantedEffectPerLevelsMax = BuildEffectPerLevels(dats);
        grantedStatSetPerLevelsMax = BuildStatSetPerLevels(dats);

        for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat64"].RowCount; monsterVarietyRow++) {

            List<string> onUpdate = new List<string>();

            var monsterVariety = dats["MonsterVarieties.dat64"][monsterVarietyRow];
            DatReference monsterTypeRef = monsterVariety["MonsterTypesKey"].GetReference();
            DatRow monsterType = monsterTypeRef.GetReferencedRow();

            /*
            int fireRes = 0; int coldRes = 0; int lightningRes = 0; int chaosRes = 0;
            DatReference? resReference = monsterType["MonsterResistancesKey"].GetReference();
            if (resReference is not null) {
                DatRow monsterResistance = resReference.GetReferencedRow();
                fireRes = monsterResistance["FireMerciless"].GetPrimitive<int>();
                coldRes = monsterResistance["ColdMerciless"].GetPrimitive<int>();
                lightningRes = monsterResistance["LightningMerciless"].GetPrimitive<int>();
                chaosRes = monsterResistance["ChaosMerciless"].GetPrimitive<int>();
            }
            */
            DatReference? resReference = monsterType["MonsterResistancesKey"].GetReference();
            int res = resReference is null ? 0 : resReference.RowIndex;

            int lifeMult = monsterVariety["LifeMultiplier"].GetPrimitive<int>();
            int ailmentMult = monsterVariety["AilmentThresholdMultiplier"].GetPrimitive<int>();
            int armourMult = monsterType["Armour"].GetPrimitive<int>();
            int evasionMult = monsterType["Evasion"].GetPrimitive<int>();
            int esMult = monsterType["EnergyShieldFromLife"].GetPrimitive<int>();

            int damageMult = monsterVariety["DamageMultiplier"].GetPrimitive<int>();
            int attackTime = monsterVariety["AttackSpeed"].GetPrimitive<int>();

            onUpdate.Add($"        SetStats(slider.value, {lifeMult}, {ailmentMult}, {armourMult}, {evasionMult}, {esMult}, {res});");

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

                        "<form>",
                        @"    <input id=""levelSlide"" type=""range"" value=""68"" min=""1"" max=""85"">Level:</input>",
                        @"    <output id=""levelOut"">68</output>",
                        "</form>",

                        HTML.Break(),

                        HTML.TableClass("block",
                            HTML.Row(HTML.Cell("Life:", "cellLife"), HTML.Cell("0", id:"life"), HTML.Cell("Ailment Threshold:", "cellLife"), HTML.Cell("0", id: "ailment")),
                            HTML.Row(HTML.Cell("Armour:", "cellPhys"), HTML.Cell("0", id: "arm"), HTML.Cell("Fire Resistance:", "cellFire"), HTML.Cell("0", id: "fire")),
                            HTML.Row(HTML.Cell("Evasion:", "cellDex"), HTML.Cell("0", id: "eva"), HTML.Cell("Cold Resistance:", "cellCold"), HTML.Cell("0", id: "cold")),
                            HTML.Row(HTML.Cell("Energy Shield:", "cellInt"), HTML.Cell("0", id: "es"), HTML.Cell("Lightning Resistance:", "cellLight"), HTML.Cell("0", id: "lightning")),
                            HTML.Row("", "", HTML.Cell("Chaos Resistance:", "cellChaos"), HTML.Cell("0", id: "chaos")) //"Damage Mult:", damageMult
                        ),
                        HTML.Break(),
                        HTML.TableClass("block", CreateMonsterModRows(monsterVariety)),
                        CreateGrantedEffectTables(monsterVariety, onUpdate)
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

            html.WriteLine(
@"<script type=""module"">
    import {SetStats} from ""./_Util.js"";
    import {SetDamage} from ""./_Util.js"";
    let slider = document.getElementById(""levelSlide"");
    function Update() {
");
            foreach (string line in onUpdate) html.WriteLine(line);
            html.WriteLine(
@"    }
    slider.addEventListener(""input"", Update );
    Update();
</script>
");
            html.Close();


            if (monsterVarietyRow % 100 == 0) Console.WriteLine(monsterVarietyRow);
        }
    }

    Dictionary<int, List<string[]>> BuildMonsterLocations() {
        Dictionary<int, List<string[]>> monsterLocations = new Dictionary<int, List<string[]>>();
        foreach (DatRow area in dats["WorldAreas.dat64"]) {
            foreach (DatReference monster in area["Bosses_MonsterVarietiesKeys"].GetReferenceArray())
                AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Boss");
            foreach (DatReference monster in area["Monsters_MonsterVarietiesKeys"].GetReferenceArray())
                AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Enemy");
        }
        foreach (DatRow row in dats["InvasionMonstersPerArea.dat64"]) {
            DatRow area = row["WorldAreasKey"].GetReference().GetReferencedRow();
            foreach (DatReference monster in row["MonsterVarietiesKeys1"].GetReferenceArray())
                AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Invasion 1");
            foreach (DatReference monster in row["MonsterVarietiesKeys2"].GetReferenceArray())
                AddMonsterLocation2(monsterLocations, monster.RowIndex, area, "Invasion 2");
        }


        foreach (DatRow row in dats["MonsterPackEntries.dat64"]) {
            int monster = row["MonsterVarietiesKey"].GetReference().RowIndex;
            DatRow pack = row["MonsterPacksKey"].GetReference().GetReferencedRow();
            foreach (DatReference areaRef in pack["WorldAreasKeys"].GetReferenceArray()) {
                DatRow area = areaRef.GetReferencedRow();
                if (!AreaIsExtraSynthesis(area)) {
                    string packName = pack["Id"].GetString();
                    AddMonsterLocation2(monsterLocations, monster, area, "Pack", packName);
                }
            }
        }

        foreach (DatRow row in dats["MonsterPacks.dat64"]) {
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
        foreach (DatRow row in dats["TableMonsterSpawners.dat64"]) {
            string id = row["Metadata"].GetString().Replace("Metadata/", "");
            foreach (DatReference monsterRef in row["SpawnsMonsters"].GetReferenceArray()) {
                if (!monsterSpawners.ContainsKey(monsterRef.RowIndex)) monsterSpawners[monsterRef.RowIndex] = new HashSet<string>();
                monsterSpawners[monsterRef.RowIndex].Add(id);
            }
        }
        foreach (int monster in monsterSpawners.Keys)
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
        DatRow monsterRow = dats["MonsterVarieties.dat64"][monster];
        string monsterDesc = $"{monsterRow["Name"].GetString()}@{type}@{monsterRow["Id"].GetString()}";
        if (!areaMonsters.ContainsKey(areaDesc)) areaMonsters[areaDesc] = new HashSet<string>();
        areaMonsters[areaDesc].Add(monsterDesc);

        string act = $"Act {area["Act"].GetPrimitive<int>()}";
        string areaName = area["Name"].GetString();
        string areaID = idReplace is null ? area["Id"].GetString() : idReplace;
        if (!monsterLocations.ContainsKey(monster)) monsterLocations[monster] = new List<string[]>();
        if (areaName == "NULL") monsterLocations[monster].Add(new string[] { type, areaID });
        else monsterLocations[monster].Add(new string[] { type, act, areaName, areaID });
    }



    Dictionary<int, HashSet<(int Monster, string Type)>> BuildMonsterRelations() {
        var monsterRelations = new Dictionary<int, HashSet<(int Monster, string Type)>>();
        foreach (DatRow row in dats["SpectreOverrides.dat64"]) {
            AddMonsterRelation(monsterRelations, row["Monster"].GetReference().RowIndex, row["Spectre"].GetReference().RowIndex, "Base", "Spectre");
        }

        //Works bad for maps with multiple bosses
        foreach (DatRow row in dats["ElderMapBossOverride.dat64"]) {
            DatRow area = row["WorldAreasKey"].GetReference().GetReferencedRow();
            foreach (DatReference bossRef in area["Bosses_MonsterVarietiesKeys"].GetReferenceArray()) {
                foreach (DatReference replacementRef in row["MonsterVarietiesKeys"].GetReferenceArray()) {
                    AddMonsterRelation(monsterRelations, bossRef.RowIndex, replacementRef.RowIndex, "Base", "Elder Boss Dummy");
                }
            }
        }

        return monsterRelations;
        //TODO redo summon relations

        //summons (god this is annoying)
        Dictionary<int, int> summonedSpecificMonstersIds = new Dictionary<int, int>();
        foreach (DatRow row in dats["SummonedSpecificMonsters.dat64"]) {
            DatReference monsterRef = row["MonsterVarietiesKey"].GetReference();
            if (monsterRef is null) summonedSpecificMonstersIds[row["Id"].GetPrimitive<int>()] = 0;
            else summonedSpecificMonstersIds[row["Id"].GetPrimitive<int>()] = monsterRef.RowIndex;
        }

        var perLevels = BuildEffectPerLevels(dats);
        for (int varietyRow = 0; varietyRow < dats["MonsterVarieties.dat64"].RowCount; varietyRow++) {
            DatRow variety = dats["MonsterVarieties.dat64"][varietyRow];
            foreach (DatReference effectRef in variety["GrantedEffectsKeys"].GetReferenceArray()) {
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
                for (int iStat = 0; iStat < statRefs.Length; iStat++) {
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
            DatRow other = dats["MonsterVarieties.dat64"][tuple.Monster];
            string link = GetMonsterCleanId(other) + ".html";
            string name = HTML.Link(link, other["Name"].GetString());
            string id = HTML.Link(link, GetMonsterCleanId(other, false));
            relations.Add(HTML.Row(tuple.Type + ':', name, id));
        }
        return HTML.TableClass("block", relations.ToArray());
    }


    string CreateGrantedEffectTables(DatRow monsterVariety, List<string> onUpdate) {
        DatReference[] refs = monsterVariety["GrantedEffectsKeys"].GetReferenceArray();
        if (refs is null) return "";
        StringBuilder effectTables = new StringBuilder();
        for (int i = 0; i < refs.Length; i++) {
            effectTables.AppendLine(CreateGrantedEffectHtml(refs[i].GetReferencedRow(), refs[i].RowIndex, onUpdate));
        }
        return effectTables.ToString();
    }

    string[] CreateMonsterModRows(DatRow monster) {
        DatReference[] refs = monster["ModsKeys"].GetReferenceArray();
        string[] text = new string[refs.Length];
        for (int i = 0; i < refs.Length; i++) {
            text[i] = HTML.Row(refs[i].GetReferencedRow().GetID());
        }
        return text;
    }

    static string[] damageStatIds = new string[] { 
        "spell_minimum_base_physical_damage", "spell_maximum_base_physical_damage" ,
        "spell_minimum_base_fire_damage", "spell_maximum_base_fire_damage" ,
        "spell_minimum_base_cold_damage", "spell_maximum_base_cold_damage" ,
        "spell_minimum_base_lightning_damage", "spell_maximum_base_lightning_damage" ,
        "spell_minimum_base_chaos_damage", "spell_maximum_base_chaos_damage"
    };
    string CreateGrantedEffectHtml(DatRow grantedEffect, int row, List<string> onUpdate) {
        float[] damageValues = new float[10];


        StringBuilder html = new StringBuilder();
        DatReference rSkill = grantedEffect["ActiveSkill"].GetReference();
        if (rSkill is null) {
            Console.WriteLine(grantedEffect["Id"].GetString() + " Has no active skill");
            return "";
        }
        DatRow activeSkill = rSkill.GetReferencedRow();
        string grantedEffectName = grantedEffect["Id"].GetString(); string skillName = activeSkill["Id"].GetString();
        DatRow grantedEffectPerLevel = grantedEffectPerLevelsMax[row];
        DatRow statSet = grantedEffect["StatSet"].GetReference().GetReferencedRow();
        DatRow grantedEffectStatsPerLevel = grantedStatSetPerLevelsMax[grantedEffect["StatSet"].GetReference().RowIndex];

        html.AppendLine("<br/><table class=\"block\">");
        html.AppendLine($"<tr><td class=\"cellGem\"><h4>{grantedEffectName} ({row})</h4></td></tr>");
        string damageType = GetSkillDamageTypes(activeSkill);
        if (damageType is not null)
            html.AppendLine($"<tr><td>{skillName} ({rSkill.RowIndex}) - {damageType}</td></tr>");
        else
            html.AppendLine($"<tr><td>{skillName} ({rSkill.RowIndex})</td></tr>");

        float baseEffectiveness = statSet["BaseEffectiveness"].GetPrimitive<float>();
        float incrementalEffectiveness = statSet["IncrementalEffectiveness"].GetPrimitive<float>();
        //html.AppendLine($"<tr><td>Effectiveness: {baseEffectiveness} {incrementalEffectiveness}</td></tr>");

        DatReference[] floatStats = grantedEffectStatsPerLevel["FloatStats"].GetReferenceArray();
        float[] floatStatValues = grantedEffectStatsPerLevel["FloatStatsValues"].GetPrimitiveArray<float>();
        int[] floatStatBaseValues = grantedEffectStatsPerLevel["BaseResolvedValues"].GetPrimitiveArray<int>();
        for (int stat = 0; stat < floatStats.Length; stat++) {
            string id = floatStats[stat].GetReferencedRow().GetID();
            int damagestat = Array.IndexOf(damageStatIds, id);
            if(damagestat != -1) {
                damageValues[damagestat] = floatStatValues[stat];
            } else
            html.AppendLine($"<tr><td  class=\"statFloat\">{floatStats[stat].GetReferencedRow().GetID()} {floatStatBaseValues[stat]} {floatStatValues[stat]}</td></tr>");
            

        }

        //Damage Lines
        for(int i = 0; i < 10; i += 2) {
            if (damageValues[i] > 0 && damageValues[i+1] > 0) {
                html.AppendLine($"<tr><td class=\"statDamage\"  id=\"{row}_{i/2}\">A</td></tr>");
                onUpdate.Add($"        SetDamage(\"{row}_{i / 2}\", slider.value, {baseEffectiveness}, {incrementalEffectiveness}, {damageValues[i]}, {damageValues[i + 1]}, {i / 2});");
            }
        }

        DatReference[] perLevelStats = grantedEffectStatsPerLevel["AdditionalStats"].GetReferenceArray();
        int[] perLevelStatValues = grantedEffectStatsPerLevel["AdditionalStatsValues"].GetPrimitiveArray<int>();
        for (int stat = 0; stat < perLevelStats.Length; stat++) {
            html.AppendLine($"<tr><td class=\"statLevel\">{perLevelStats[stat].GetReferencedRow().GetID()} {perLevelStatValues[stat]}</td></tr>");
        }

        DatReference[] constantStats = statSet["ConstantStats"].GetReferenceArray();
        int[] constantStatValues = statSet["ConstantStatsValues"].GetPrimitiveArray<int>();
        for (int stat = 0; stat < constantStats.Length; stat++) {
            html.AppendLine($"<tr><td class=\"statConst\">{GetStatDescription(constantStats[stat].GetReferencedRow(), constantStatValues[stat])}</td></tr>");
        }

        foreach (DatReference staticStatRef in statSet["ImplicitStats"].GetReferenceArray()) {
            html.AppendLine($"<tr><td class=\"statTag\">{staticStatRef.GetReferencedRow()["Id"].GetString()}</td></tr>");
        }
        html.Append("</table>");
        return html.ToString();
    }



    string GetStatDescription(DatRow stat, int intStatValue) {
        string id = stat["Id"].GetString();
        if (id == "alternate_minion") {
            for (int summonRow = 0; summonRow < dats["SummonedSpecificMonsters.dat64"].RowCount; summonRow++) {
                DatRow row = dats["SummonedSpecificMonsters.dat64"][summonRow];
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
        return $"{id} {intStatValue}";
    }

    string GetMonsterCleanId(DatRow monsterVariety, bool replaceSlashes = true) {
        if (replaceSlashes) return monsterVariety["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_').Replace('/', '_');
        return monsterVariety["Id"].GetString().Replace("Metadata/Monsters/", "").TrimEnd('_');
    }


    string GetRigFromAO(string path) {
        //super hacky
        if (!File.Exists(path)) return "COULD NOT FIND RIG";
        foreach (string line in File.ReadAllLines(path)) {
            if (line.Contains("metadata =")) {
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


    string GetSkillDamageTypes(DatRow activeSkill) {
        HashSet<int> contextFlags = new HashSet<int>();
        foreach (DatReference contextFlagRef in activeSkill["VirtualStatContextFlags"].GetReferenceArray()) contextFlags.Add(contextFlagRef.RowIndex);
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




    string ListStrings(params object[] vals) {
        StringBuilder s = new StringBuilder();
        for (int i = 0; i < vals.Length; i++) { s.Append(vals[i].ToString()); s.Append('\t'); }
        return s.ToString();
    }



    Dictionary<int, DatRow> BuildEffectPerLevels(DatFileIndex dats) {
        Dictionary<int, DatRow> effectPerLevels = new Dictionary<int, DatRow>();

        foreach (DatRow row in dats["GrantedEffectsPerLevel.dat64"]) {
            int grantedEffect = row["GrantedEffect"].GetReference().RowIndex;
            if (!effectPerLevels.ContainsKey(grantedEffect)) effectPerLevels[grantedEffect] = row;
            else {
                if (row["Level"].GetPrimitive<int>() > effectPerLevels[grantedEffect]["Level"].GetPrimitive<int>()) effectPerLevels[grantedEffect] = row;
            }
        }

        return effectPerLevels;
    }


    Dictionary<int, DatRow> BuildStatSetPerLevels(DatFileIndex dats) {
        Dictionary<int, DatRow> statSetPerLevels = new Dictionary<int, DatRow>();

        foreach (DatRow row in dats["GrantedEffectStatSetsPerLevel.dat64"]) {
            int statSet = row["StatSet"].GetReference().RowIndex;
            if (!statSetPerLevels.ContainsKey(statSet)) statSetPerLevels[statSet] = row;
            else {
                if (row["PlayerLevelReq"].GetPrimitive<int>() > statSetPerLevels[statSet]["PlayerLevelReq"].GetPrimitive<int>()) statSetPerLevels[statSet] = row;
            }
        }
        return statSetPerLevels;
    }
}

