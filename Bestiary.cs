using System;
using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;
using Archbestiary.Util;
using PoeTerrain;
using System.Runtime.InteropServices;
using System.Data;

public class Bestiary {
    Dictionary<int, List<DatRow>> grantedEffectPerLevels;
    Dictionary<int, List<DatRow>> grantedStatSetPerLevels;
    Dictionary<string, HashSet<string>> areaMonsters = new Dictionary<string, HashSet<string>>();
    //DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemaformatted.json");
    //DatFileIndex dats = new DatFileIndex(new DiskDirectory(@"E:\Extracted\PathOfExile\3.18.Sentinel\Data\"), spec);
    public DatSpecIndex spec;
    public DatFileIndex dats;



    //ListPacks();
    //return;

    public Bestiary(string path = @"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\Data\", string schema = @"E:\Anna\Downloads\schema.min(7).json") {
        spec = DatSpecIndex.Create(schema);
        dats = new DatFileIndex(new DiskDirectory(path), spec);
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

                int armourMult = monsterType["Armour"].GetPrimitive<int>();
                int evasionMult = monsterType["Evasion"].GetPrimitive<int>();
                int esMult = monsterType["EnergyShieldFromLife"].GetPrimitive<int>();

                string name = monsterVariety["Name"].GetString();
                if (name.Length >= 35) name = name.Substring(0, 35);
                html.AppendLine($"<tr><td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9,lifeMult/100)}\" target=\"body\">{lifeMult}</a></td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9,(damageMult-50)/50)}\" target=\"body\">{damageMult}</a></td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, armourMult / 10)}\" target=\"body\">{armourMult}</a></td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, evasionMult / 10)}\" target=\"body\">{evasionMult}</a></td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, esMult / 10)}\" target=\"body\">{esMult}</a></td>");
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

        grantedEffectPerLevels = BuildEffectPerLevels(dats);
        grantedStatSetPerLevels = BuildStatSetPerLevels(dats);

        for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat64"].RowCount; monsterVarietyRow++) {

            List<string> onUpdate = new List<string>();
            HashSet<string> usedFunctions = new HashSet<string>();

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
            int damageSpread = monsterType["DamageSpread"].GetPrimitive<int>();

            onUpdate.Add($"        SetStats(slider.value, {lifeMult}, {ailmentMult}, {armourMult}, {evasionMult}, {esMult}, {res});");
            usedFunctions.Add("SetStats");

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
            string[] rigs = new string[aos.Length]; for (int ao = 0; ao < aos.Length; ao++) rigs[ao] = GetRigFromAO(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\", aos[ao]);
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
                            HTML.Row("", "", HTML.Cell("Chaos Resistance:", "cellChaos"), HTML.Cell("0", id: "chaos")), //"Damage Mult:", damageMult
                            HTML.Row("Damage Mult:", damageMult, "Damage Spread", damageSpread) //"Damage Mult:", damageMult

                        ),
                        HTML.Break(),
                        HTML.TableClass("block", CreateMonsterModRows(monsterVariety)),
                        CreateGrantedEffectTables(monsterVariety, onUpdate, usedFunctions, damageMult, damageSpread)
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

            StringBuilder funcS = new StringBuilder();
            foreach(string func in usedFunctions) funcS.Append(func + ", ");
            funcS.Remove(funcS.Length - 2, 2);

            html.WriteLine(
$@"<script type=""module"">
    import {{{funcS}}} from ""./_Util.js"";
    let slider = document.getElementById(""levelSlide"");
    function Update() {{
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


        /* TODO per level stats properly
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
        */

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


    string CreateGrantedEffectTables(DatRow monsterVariety, List<string> onUpdate, HashSet<string> usedFunctions, int damageMult, int damageSpread) {
        DatReference[] refs = monsterVariety["GrantedEffectsKeys"].GetReferenceArray();
        if (refs is null) return "";
        StringBuilder effectTables = new StringBuilder();
        for (int i = 0; i < refs.Length; i++) {
            effectTables.AppendLine(CreateGrantedEffectHtml(refs[i].GetReferencedRow(), refs[i].RowIndex, onUpdate, usedFunctions, damageMult, damageSpread));
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
    static string[] dotStatIds = new string[] {
        "base_physical_damage_to_deal_per_minute",
        "base_fire_damage_to_deal_per_minute",
        "base_cold_damage_to_deal_per_minute",
        "base_lightning_damage_to_deal_per_minute",
        "base_chaos_damage_to_deal_per_minute",
    };
    string CreateGrantedEffectHtml(DatRow grantedEffect, int row, List<string> onUpdate, HashSet<string> usedFunctions, int damageMult = 100, int damageSpread = 20, bool debug = true) {
        float[] damageValues = new float[10];


        StringBuilder w = new StringBuilder();


        w.AppendLine("<br/><table class=\"block\">");
        w.AppendLine(HTML.Row(HTML.Cell($"<h4>{grantedEffect.GetID()} ({row})</h4>", "cellGem")));
        w.AppendLine(HTML.Row(HTML.Cell($"Cast Time: {grantedEffect["CastTime"].GetPrimitive<int>()}")));


        //ActiveSkill
        {
            if(debug) w.AppendLine(HTML.Row(HTML.Cell("ActiveSkill", "cellFire")));
            DatReference rSkill = grantedEffect["ActiveSkill"].GetReference();
            DatRow activeSkill = rSkill.GetReferencedRow();
            int skillId = rSkill.RowIndex;
            string skillName = activeSkill.GetID();
            string damageType = GetSkillDamageTypes(activeSkill);
            if (damageType is not null)
                w.AppendLine($"<tr><td>{skillName} ({skillId}) - {damageType}</td></tr>");
            else
                w.AppendLine($"<tr><td>{skillName} ({skillId})</td></tr>");

        }


        //GrantedEffectStatSets

        
        if (debug) w.AppendLine(HTML.Row(HTML.Cell("GrantedEffectStatSet", "cellFire")));
        DatRow statSet = grantedEffect["StatSet"].GetReference().GetReferencedRow();
        float baseEffectiveness = statSet["BaseEffectiveness"].GetPrimitive<float>();
        float incrementalEffectiveness = statSet["IncrementalEffectiveness"].GetPrimitive<float>();
        w.AppendLine($"<tr><td>Effectiveness: {baseEffectiveness} {incrementalEffectiveness}</td></tr>");

        {
            DatReference[] constantStats = statSet["ConstantStats"].GetReferenceArray();
            int[] constantStatValues = statSet["ConstantStatsValues"].GetPrimitiveArray<int>();
            for (int stat = 0; stat < constantStats.Length; stat++) {
                w.AppendLine($"<tr><td class=\"statConst\">{GetStatDescription(constantStats[stat].GetReferencedRow(), constantStatValues[stat])}</td></tr>");
            }
            foreach (DatReference staticStatRef in statSet["ImplicitStats"].GetReferenceArray()) {
                w.AppendLine($"<tr><td class=\"statTag\">{staticStatRef.GetReferencedRow()["Id"].GetString()}</td></tr>");
            }
        }


        //GrantedEffectsPerLevel
        {

            if (debug) w.AppendLine(HTML.Row(HTML.Cell("GrantedEffectsPerLevel", "cellFire")));

            var levels = grantedEffectPerLevels[row];

            int attackSpeedMult = levels[0].GetInt("AttackSpeedMultiplier");  //technically changes for like 3 things
            w.AppendLine(HTML.RowList("Attack Speed Mult: " + attackSpeedMult.ToString()));


            int cooldownGroup = levels[0].GetInt("CooldownGroup");  //technically changes for like 1 thing but I think its a bug
            List<int> levelReqs = new List<int>(); levelReqs.Add(levels[0].GetInt("PlayerLevelReq"));
            List<int> storedUses = new List<int>(); storedUses.Add(levels[0].GetInt("StoredUses"));
            List<int> cooldowns = new List<int>(); cooldowns.Add(levels[0].GetInt("Cooldown"));



            for (int i = 1; i < levels.Count; i++) {
                int newLevelReq = levels[i].GetInt("PlayerLevelReq");
                int newStoredUses = levels[i].GetInt("StoredUses");
                int newCooldown = levels[i].GetInt("Cooldown");
                if(newStoredUses != storedUses[storedUses.Count - 1] || newCooldown != cooldowns[cooldowns.Count - 1]) {
                    levelReqs.Add(newLevelReq); storedUses.Add(newStoredUses); cooldowns.Add(newCooldown);
                }
            }

            for (int i = 0; i < storedUses.Count; i++)
                if (storedUses[i] != 0) {
                    if (levelReqs.Count > 1) {
                        w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_c")));
                        onUpdate.Add(@$"		SetCooldown(""{row}_c"", slider.value, {HTML.JSArray(levelReqs.ToArray())}, {HTML.JSArray(storedUses.ToArray())}, {HTML.JSArray(cooldowns.ToArray())});");
                        usedFunctions.Add("SetCooldown");
                    } else {
                        if (storedUses[0] > 1) w.AppendLine(HTML.Row(HTML.Cell($"Cooldown Time: {((float)cooldowns[0])/1000} sec ({storedUses[0]} uses)", "statTag")));
                        else w.AppendLine(HTML.Row(HTML.Cell($"Cooldown Time: {((float)cooldowns[0]) / 1000} sec", "statTag")));
                    }
                    break;
                }
        
        }

        //GrantedEffectStatSetsPerLevel
        {
            if (debug) w.AppendLine(HTML.Row(HTML.Cell("GrantedEffectStatSetsPerLevel", "cellFire")));

            int statSetIndex = grantedEffect["StatSet"].GetReference().RowIndex;
            var levels = grantedStatSetPerLevels[statSetIndex];

            //int spellCritChance
            //int baseMultiplier = levels[0].GetInt("BaseMultiplier"); changes for attacks only, does this mean its not used for spells or is that just because the scaling is already handled by effectiveness?
            //InterpolationBases - does not change
            //StatInterpolations - changes size

            List<int> additionalFlagLevels = new List<int>(); additionalFlagLevels.Add(levels[0].GetInt("PlayerLevelReq"));
            List<string> additionalFlags = new List<string>(); additionalFlags.Add(levels[0].GetReferenceArrayIDsFormatted("AdditionalFlags"));




            Dictionary<int, List<int>> intStatLevels = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> intStatValues = new Dictionary<int, List<int>>();

            {
                var intStats = levels[0]["AdditionalStats"].GetReferenceArray();
                var intValues = levels[0]["AdditionalStatsValues"].GetPrimitiveArray<int>();
                for(int i = 0; i < intStats.Length; i++) {
                    intStatLevels[intStats[i].RowIndex] = new List<int> { levels[0].GetInt("PlayerLevelReq") };
                    intStatValues[intStats[i].RowIndex] = new List<int> { intValues[i] };
                }

            }


            Dictionary<int, List<int>> floatStatLevels = new Dictionary<int, List<int>>();
            Dictionary<int, List<float>> floatStatValues = new Dictionary<int, List<float>>();
            {
                var floatStats = levels[0]["FloatStats"].GetReferenceArray();
                var floatValues = levels[0]["FloatStatsValues"].GetPrimitiveArray<float>();
                for (int i = 0; i < floatStats.Length; i++) {
                    floatStatLevels[floatStats[i].RowIndex] = new List<int> { levels[0].GetInt("PlayerLevelReq") };
                    floatStatValues[floatStats[i].RowIndex] = new List<float> { floatValues[i] };
                }

            }

            //string test = "BaseMultiplier";
            for (int i = 1; i < levels.Count; i++) {
                int level = levels[i].GetInt("PlayerLevelReq");


                string newAdditionalFlags = levels[i].GetReferenceArrayIDsFormatted("AdditionalFlags");
                if (newAdditionalFlags != additionalFlags[additionalFlags.Count - 1]) {
                    additionalFlags.Add(newAdditionalFlags);
                    additionalFlagLevels.Add(level);
                }

                {
                    var intStats = levels[i]["AdditionalStats"].GetReferenceArray();
                    var intValues = levels[i]["AdditionalStatsValues"].GetPrimitiveArray<int>();
                    for (int stat = 0; stat < intStats.Length; stat++) {

                        int statIndex = intStats[stat].RowIndex;

                        if (!intStatLevels.ContainsKey(statIndex)) {
                            intStatLevels[statIndex] = new List<int>() { 0 };
                            intStatValues[statIndex] = new List<int>() { 887887 }; //use as a "hide this" value
                        }
                        if (intStatValues[statIndex][intStatValues[statIndex].Count - 1] != intValues[stat]) {  //TODO if interpolation is 2 we need to keep all levels, including ones with the same value
                            intStatLevels[statIndex].Add(level);
                            intStatValues[statIndex].Add(intValues[stat]);
                        }
                    }
                }


                var floatStats = levels[i]["FloatStats"].GetReferenceArray();
                var floatValues = levels[i]["FloatStatsValues"].GetPrimitiveArray<float>();
                for (int stat = 0; stat < floatStats.Length; stat++) {
                    int statIndex = floatStats[stat].RowIndex;
                    if (!floatStatLevels.ContainsKey(statIndex))  Console.WriteLine("FLOAT STAT ADDED, THIS SHOULD NEVER HAPPEN");
                    else if (floatStatValues[statIndex][floatStatValues[statIndex].Count - 1] != floatValues[stat]) {
                        floatStatLevels[statIndex].Add(level);
                        floatStatValues[statIndex].Add(floatValues[stat]);
                    }
                }


                //var oldVals = levels[i - 1]["AdditionalStatsValues"].GetPrimitiveArray<int>();
                //var newVals = levels[i]["AdditionalStatsValues"].GetPrimitiveArray<int>();
                //if(oldVals.Length != newVals.Length) Console.WriteLine($"CHANGESIZE {grantedEffect.GetID()} {string.Concat(oldVals)} {string.Concat(newVals)}");
                //else for (int f = 0; f <  newVals.Length; f++) if (oldVals[f] != newVals[f]) Console.WriteLine($"{grantedEffect.GetID()} {newVals[f]} {oldVals[f]}");


                //if (levels[i].GetInt(test) != levels[i - 1].GetInt(test)) Console.WriteLine($"{grantedEffect.GetID()} {test} {levels[i].GetInt(test)} {levels[i - 1].GetInt(test)}");
            }


            foreach (int statRow in intStatLevels.Keys) {
                w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_{statRow}")));
                onUpdate.Add(@$"		SetIntStat(""{row}_{statRow}"", slider.value, ""{dats["Stats.dat64"][statRow].GetID()}"", {HTML.JSArray(intStatLevels[statRow].ToArray())}, {HTML.JSArray(intStatValues[statRow].ToArray())});");
                usedFunctions.Add("SetIntStat");
            }

            foreach(int statRow in floatStatLevels.Keys) {
                w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_{statRow}")));
                onUpdate.Add(@$"		SetFloatStat(""{row}_{statRow}"", slider.value, ""{dats["Stats.dat64"][statRow].GetID()}"", {HTML.JSArray(floatStatLevels[statRow].ToArray())}, {HTML.JSArray(floatStatValues[statRow].ToArray())}, {baseEffectiveness}, {incrementalEffectiveness});");
                usedFunctions.Add("SetFloatStat");
            }


            if (additionalFlags.Count == 1) {
                if (additionalFlags[0] != "")
                    Console.WriteLine("CONSTANT ADDITIONALFLAGS (THIS SHOULD NEVER HAPPEN)");
            } else {
                w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_f")));
                onUpdate.Add(@$"		SetLevelText(""{row}_f"", slider.value, {HTML.JSArray(additionalFlagLevels.ToArray())}, {HTML.JSArray(additionalFlags.ToArray())});");
                usedFunctions.Add("SetLevelText");
            }

        }

        /*

        DatRow grantedEffectPerLevel = grantedEffectPerLevels[row][0]; //TODO
        DatRow grantedEffectStatsPerLevel = grantedStatSetPerLevelsMax[grantedEffect["StatSet"].GetReference().RowIndex][0];



        //base damage (for attacks)
        foreach (DatReference contextFlagRef in activeSkill["VirtualStatContextFlags"].GetReferenceArray()) 
            if(contextFlagRef.RowIndex == 2) {
                int attackMult = (10000 + grantedEffectStatsPerLevel["BaseMultiplier"] + 50) / 100;
                int damageEffectiveness = (10000 + grantedEffectStatsPerLevel["DamageEffectiveness"] + 50) / 100;
                w.AppendLine(HTML.Row(HTML.Cell($"Attack Damage: {attackMult}% of base", "statDamage")));
                w.AppendLine(HTML.Row(HTML.Cell($"Damage Effectiveness: {damageEffectiveness}% of base", "statDamage")));
                w.AppendLine(HTML.Row(HTML.Cell("A", "statDamage", $"{row}_a")));
                onUpdate.Add(@$"		SetAttack(""{row}_a"", slider.value, {damageMult}, {damageSpread}, {attackMult});");
                break;
            }


        DatReference[] floatStats = grantedEffectStatsPerLevel["FloatStats"].GetReferenceArray();
        float[] floatStatValues = grantedEffectStatsPerLevel["FloatStatsValues"].GetPrimitiveArray<float>();
        int[] floatStatBaseValues = grantedEffectStatsPerLevel["BaseResolvedValues"].GetPrimitiveArray<int>();
        for (int stat = 0; stat < floatStats.Length; stat++) {
            string id = floatStats[stat].GetReferencedRow().GetID();
            int damagestat = Array.IndexOf(damageStatIds, id);
            if(damagestat != -1) {
                damageValues[damagestat] = floatStatValues[stat];
            } else {
                damagestat = Array.IndexOf(dotStatIds, id);
                if(damagestat != -1) {
                    w.AppendLine($"<tr><td class=\"statDamage\"  id=\"{row}_d{damagestat}\">A</td></tr>");
                    onUpdate.Add($"        SetDot(\"{row}_d{damagestat}\", slider.value, {baseEffectiveness}, {incrementalEffectiveness}, {floatStatValues[stat]}, {damagestat});");
                } else {
                    w.AppendLine($"<tr><td  class=\"statFloat\">{floatStats[stat].GetReferencedRow().GetID()} {floatStatBaseValues[stat]} {floatStatValues[stat]}</td></tr>");
                }
            }
            

        }

        //Damage Lines
        for(int i = 0; i < 10; i += 2) {
            if (damageValues[i] > 0 && damageValues[i+1] > 0) {
                w.AppendLine($"<tr><td class=\"statDamage\"  id=\"{row}_{i/2}\">A</td></tr>");
                onUpdate.Add($"        SetDamage(\"{row}_{i / 2}\", slider.value, {baseEffectiveness}, {incrementalEffectiveness}, {damageValues[i]}, {damageValues[i + 1]}, {i / 2});");
            }
        }

        DatReference[] perLevelStats = grantedEffectStatsPerLevel["AdditionalStats"].GetReferenceArray();
        int[] perLevelStatValues = grantedEffectStatsPerLevel["AdditionalStatsValues"].GetPrimitiveArray<int>();
        for (int stat = 0; stat < perLevelStats.Length; stat++) {
            w.AppendLine($"<tr><td class=\"statLevel\">{perLevelStats[stat].GetReferencedRow().GetID()} {perLevelStatValues[stat]}</td></tr>");
        }
        */


        w.Append("</table>");
        return w.ToString();
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


    public static string GetRigFromAO(string metadatafolder, string path) {
        //super hacky
        string combined = Path.Combine(metadatafolder, path);
        if (!File.Exists(combined)) return "COULD NOT FIND " + combined;
        string rig = null;
        string parent = null;
        foreach (string line in File.ReadAllLines(combined)) {
            if(line.StartsWith("extends \"")) {
                parent = line.Split('"')[1];
            }

            if (line.Contains("metadata =")) {
                rig = line.Substring(line.IndexOf('"')).Trim('"');
                break;
            }
        }
        if(rig == null && parent != null) {
            if (parent != null) rig = GetRigFromAO(metadatafolder, parent + ".ao");
            else rig = "COULD NOT FIND RIG";
        }

        return rig;
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
        HashSet<int> activeSkillTypes = new HashSet<int>();
        foreach (var activeSkillType in activeSkill["ActiveSkillTypes"].GetReferenceArray()) 
            activeSkillTypes.Add(activeSkillType.RowIndex);
        StringBuilder s = new StringBuilder();
        if (activeSkillTypes.Contains(0)) s.Append("Attack, ");
        if (activeSkillTypes.Contains(1)) s.Append("Spell, ");
        if (activeSkillTypes.Contains(2)) s.Append("Projectile, ");
        if (activeSkillTypes.Contains(7)) s.Append("Area, ");
        if (s.Length > 0) { 
            s.Remove(s.Length - 2, 2); 
            return s.ToString(); 
        } return null;


        /*
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
        */
    }




    string ListStrings(params object[] vals) {
        StringBuilder s = new StringBuilder();
        for (int i = 0; i < vals.Length; i++) { s.Append(vals[i].ToString()); s.Append('\t'); }
        return s.ToString();
    }



    public Dictionary<int, List<DatRow>> BuildEffectPerLevels(DatFileIndex dats) {
        Dictionary<int, List<DatRow>> effectPerLevels = new Dictionary<int, List<DatRow>>();

        foreach (DatRow row in dats["GrantedEffectsPerLevel.dat64"]) {
            int level = row["PlayerLevelReq"].GetPrimitive<int>();
            int grantedEffect = row["GrantedEffect"].GetReference().RowIndex;
            if (!effectPerLevels.ContainsKey(grantedEffect)) effectPerLevels[grantedEffect] = new List<DatRow>();
            int insert = 0;
            for(int i = 0; i < effectPerLevels[grantedEffect].Count; i++) {
                int checkLevel = effectPerLevels[grantedEffect][i]["PlayerLevelReq"].GetPrimitive<int>();
                if (level == checkLevel) { //higher level gem with same level requirement, only happens on player skills
                    insert = -1;
                    effectPerLevels[grantedEffect][i] = row;
                    break;
                } else if (checkLevel < level) insert++;
            }
            if(insert >= 0) effectPerLevels[grantedEffect].Insert(insert, row);
        }



        return effectPerLevels;
    }


    public Dictionary<int, List<DatRow>> BuildStatSetPerLevels(DatFileIndex dats) {
        Dictionary<int, List<DatRow>> statSetPerLevels = new Dictionary<int, List<DatRow>>();

        foreach (DatRow row in dats["GrantedEffectStatSetsPerLevel.dat64"]) {
            int level = row["PlayerLevelReq"].GetPrimitive<int>();
            int statSet = row["StatSet"].GetReference().RowIndex;
            if (!statSetPerLevels.ContainsKey(statSet)) statSetPerLevels[statSet] = new List<DatRow>();
            int insert = 0;
            for (int i = 0; i < statSetPerLevels[statSet].Count; i++) {
                int checkLevel = statSetPerLevels[statSet][i]["PlayerLevelReq"].GetPrimitive<int>();
                if (level == checkLevel) { //higher level gem with same level requirement, only happens on player skills
                    insert = -1;
                    statSetPerLevels[statSet][i] = row;
                    break;
                } else if (checkLevel < level) insert++;
            }
            if (insert >= 0) statSetPerLevels[statSet].Insert(insert, row);
        }

        return statSetPerLevels;

    }
}

