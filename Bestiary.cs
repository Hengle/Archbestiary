using System;
using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;
using Archbestiary.Util;
using PoeFormats;
using System.Data;
using Microsoft.VisualBasic.FileIO;

public class Bestiary {
    Dictionary<string, HashSet<string>> areaMonsters = new Dictionary<string, HashSet<string>>();
    //DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemaformatted.json");
    //DatFileIndex dats = new DatFileIndex(new DiskDirectory(@"E:\Extracted\PathOfExile\3.18.Sentinel\Data\"), spec);
    public DatSpecIndex spec;
    public DatFileIndex dats;
    Dictionary<string, ObjectTemplate> monsterOTs;
    string basePath;
    GrantedEffects grantedEffacts;

    enum MonsterCategories {
        Act1,
        Act2,
        Act3,
        Act4,
        Act5,
        Act6,
        Act7,
        Act8,
        Act9,
        Act10,
        Atlas,
        Anarchy,
        Sacrifice,
        Torment,
        Breach,
        Harbinger,
        Uncategorized
    }

    static string[] monsterCategoryNames = new string[] { 
        "Act 1", "Act 2", "Act 3", "Act 4", "Act 5", "Act 6", "Act 7", "Act 8", "Act 9", "Act 10", 
        "Atlas", "Anarchy", "Sacrifice", "Torment", "Breach", "Harbinger",  "Uncategorized" };

    //ListPacks();
    //return;

    public Bestiary(string path = @"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\", string schema = null) {
        if(schema == null) 
            for(int i = 100; i > 0; i--) 
                if(File.Exists($@"E:\A\Downloads\schema.min({i}).json")) {
                    schema = $@"E:\A\Downloads\schema.min({i}).json";
                    break;
                }
        basePath = path;
        spec = DatSpecIndex.Create(schema);
        dats = new DatFileIndex(new DiskDirectory(Path.Combine(path, "data")), spec);
        grantedEffacts = new GrantedEffects(dats);
    }

    //CreateMonsterList();

    public void CreateMonsterList() {

        HashSet<int> bosses = new HashSet<int>();
        foreach (DatRow row in dats["WorldAreas.dat64"]) {
            foreach (DatReference boss in row["Bosses_MonsterVarietiesKeys"].GetReferenceArray()) {
                bosses.Add(boss.RowIndex);
            }
        }
        Console.WriteLine(bosses.Count);


        HashSet<int> mapMonsters = GetMapMonsters();
        int[] monsterCategories = GetMonsterCategories(bosses);


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
                html.AppendLine($"<tr><td>{(mapMonsters.Contains(monsterVarietyRow) ? "<img src=\"m.png\"/>" : "")}</td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9,lifeMult/100)}\" target=\"body\">{lifeMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9,(damageMult-50)/50)}\" target=\"body\">{damageMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, armourMult / 10)}\" target=\"body\">{armourMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, evasionMult / 10)}\" target=\"body\">{evasionMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, esMult / 10)}\" target=\"body\">{esMult}</a></td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{added}</a></td>");
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
        File.WriteAllText(@"E:\A\A\Visual Studio\Archbestiary\web\index.html", html.ToString());
    }

    public void CreateMonsterListNew() {

        HashSet<int> bosses = new HashSet<int>();
        foreach (DatRow row in dats["WorldAreas.dat64"]) {
            foreach (DatReference boss in row["Bosses_MonsterVarietiesKeys"].GetReferenceArray()) {
                bosses.Add(boss.RowIndex);
            }

        }
        Console.WriteLine(bosses.Count);


        HashSet<int> mapMonsters = GetMapMonsters();
        int[] monsterCategories = GetMonsterCategories(bosses);
        //Dictionary<string, string> monsterAdded = History.BuildMonsterVarietyHistory(true);
        Dictionary<string, string> monsterAdded = new Dictionary<string, string>();

        StringBuilder html = new StringBuilder();
        html.AppendLine("<link rel=\"stylesheet\" href=\"index.css\"></link>");
        html.AppendLine("<body><div class=\"maindiv\">");

        HashSet<int> ignore = new HashSet<int>();

        foreach (DatRow row in dats["SpectreOverrides.dat64"])
            ignore.Add(row["Spectre"].GetReference().RowIndex);

        foreach (DatRow row in dats["ElderMapBossOverride.dat64"])
            foreach (DatReference bossRef in row["MonsterVarietiesKeys"].GetReferenceArray())
                ignore.Add(bossRef.RowIndex);



        for(int category = 0; category < monsterCategoryNames.Length; category++) {
            html.AppendLine($"<h3>{monsterCategoryNames[category]}</h3>");
            html.AppendLine("<table>");
            foreach(DatRow monster in dats["MonsterVarieties.dat64"]) {

                if (monsterCategories[monster.rowIndex] != category) continue;
                string id = monster.GetID().TrimEnd('_');
                string added = monsterAdded.ContainsKey(id) ? monsterAdded[id] : "U";
                id = id.Replace("Metadata/Monsters/", "");

                string monsterClass = bosses.Contains(monster.rowIndex) ? "linkboss" : "linknormal";
                string icon = mapMonsters.Contains(monster.rowIndex) ? "<img src=\"m.png\"/>" : "";

                bool fireRes = false;
                bool coldRes = false;
                bool lightRes = false;
                bool chaosRes = false;
                DatReference? resReference = monster.GetRef("MonsterTypesKey").GetReferencedRow().GetRef("MonsterResistancesKey");
                if(resReference is not null) {
                    DatRow res = resReference.GetReferencedRow();
                    fireRes = res.GetInt("FireMerciless") > 0;
                    coldRes = res.GetInt("ColdMerciless") > 0;
                    lightRes = res.GetInt("LightningMerciless") > 0;
                    chaosRes = res.GetInt("ChaosMerciless") > 0;
                }

                int lifeMult = monster.GetInt("LifeMultiplier");


                bool[] damageTypes = grantedEffacts.GetDamageTypes(monster);

                string name = monster.GetName();
                if (name.Length >= 35) name = name.Substring(0, 35);

                html.AppendLine(HTML.Row(
                    HTML.Cell(icon, "icon"),
                    $"<a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a>",
                    HTML.Cell(damageTypes[0] ? "<img src=\"phys.png\"/>" : "<img src=\"physg.png\"/>", "icon"),
                    HTML.Cell(damageTypes[1] ? "<img src=\"fire.png\"/>" : "<img src=\"fireg.png\"/>", "icon"),
                    HTML.Cell(damageTypes[2] ? "<img src=\"cold.png\"/>" : "<img src=\"coldg.png\"/>", "icon"),
                    HTML.Cell(damageTypes[3] ? "<img src=\"light.png\"/>" : "<img src=\"lightg.png\"/>", "icon"),
                    HTML.Cell(damageTypes[4] ? "<img src=\"chaos.png\"/>" : "<img src=\"chaosg.png\"/>", "icon"),
                    HTML.Cell(lifeMult.ToString(), $"m{Math.Min(9, lifeMult / 100)}"),
                    HTML.Cell(fireRes ? "<img src=\"fire.png\"/>" : "<img src=\"fireg.png\"/>", "icon"),
                    HTML.Cell(coldRes ? "<img src=\"cold.png\"/>" : "<img src=\"coldg.png\"/>", "icon"),
                    HTML.Cell(lightRes ? "<img src=\"light.png\"/>" : "<img src=\"lightg.png\"/>", "icon"),
                    HTML.Cell(chaosRes ? "<img src=\"chaos.png\"/>" : "<img src=\"chaosg.png\"/>", "icon"),
                    $"<a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{added}</a>",
                    $"<a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{id}</a>"));
            }
            html.AppendLine("</table>");
        }
        /*
        html.AppendLine($"<h3>Uncategorised</h3>");

        html.AppendLine("<table>");
        
        for (int monsterVarietyRow = 1; monsterVarietyRow < dats["MonsterVarieties.dat64"].RowCount; monsterVarietyRow++) {
            if (!ignore.Contains(monsterVarietyRow)) {
                if (monsterCategories[monsterVarietyRow] != 0) continue;
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
                html.AppendLine($"<tr><td>{(mapMonsters.Contains(monsterVarietyRow) ? "<img src=\"m.png\"/>" : "")}</td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{name}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9,lifeMult/100)}\" target=\"body\">{lifeMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9,(damageMult-50)/50)}\" target=\"body\">{damageMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, armourMult / 10)}\" target=\"body\">{armourMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, evasionMult / 10)}\" target=\"body\">{evasionMult}</a></td>");
                //html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"m{Math.Min(9, esMult / 10)}\" target=\"body\">{esMult}</a></td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{added}</a></td>");
                html.AppendLine($"<td><a href=\"Monsters/{id.Replace('/', '_')}.html\" class=\"{monsterClass}\" target=\"body\">{id}</a></td></tr>");
            }
        }
        html.Append("</table>");
        */
        html.Append("</details><details><summary>Summoned</summary><table>");


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
        File.WriteAllText(@"E:\A\A\Visual Studio\Archbestiary\web\index.html", html.ToString());
    }


    public void CreateMonsterPages() {

        Dictionary<string, string> astAnimations = new Dictionary<string, string>();

        var monsterLocations = BuildMonsterLocations();
        var monsterRelations = BuildMonsterRelations();
        monsterOTs = new Dictionary<string, ObjectTemplate>();

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
            int accuracyMult = monsterType.GetInt("Accuracy");
            int damageMult = monsterVariety["DamageMultiplier"].GetPrimitive<int>();
            int attackTime = monsterVariety["AttackSpeed"].GetPrimitive<int>();
            int damageSpread = monsterType["DamageSpread"].GetPrimitive<int>();

            onUpdate.Add($"        SetStats(slider.value, {lifeMult}, {ailmentMult}, {armourMult}, {evasionMult}, {esMult}, {accuracyMult}, {res});");
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
            string[] rigs = new string[aos.Length]; for (int ao = 0; ao < aos.Length; ao++) rigs[ao] = GetRigFromAO(basePath, aos[ao]);

            string[] acts = monsterVariety["ACTFiles"].GetStringArray();
            //Act act = new Act(Path.Combine(basePath, acts[0]));

            //StringBuilder animationTextBuilder = new StringBuilder(acts[0]);
            //foreach(var action in act.animations.Keys) {
            //    animationTextBuilder.AppendLine(HTML.RowList($"{action} - {act.animations[action]}"));
            //}
            string animationText = HTML.RowList(File.ReadAllText(Path.Combine(basePath, acts[0])));

            /*
            string animationText = HTML.RowList(rigs[0]);
            
            if (rigs[0] != "COULD NOT FIND RIG") {
                string astPath = Path.Combine(basePath, rigs[0]).Replace(".amd", ".ast");
                animationText = HTML.RowList(astPath);
                if (astAnimations.ContainsKey(astPath)) {
                    animationText = astAnimations[astPath];
                } else if (File.Exists(astPath)) {
                    Ast ast = new Ast(astPath);
                    StringBuilder s = new StringBuilder();
                    for (int i = 0; i < ast.animations.Length; i++) s.AppendLine(HTML.RowList(ast.animations[i].name));
                    animationText = s.ToString();
                    astAnimations[astPath] = animationText;
                }
            }
            */



            for (int ao = 0; ao < aos.Length; ao++) aos[ao] = aos[ao].Replace("Metadata/", "");
            for (int rig = 0; rig < rigs.Length; rig++) rigs[rig] = rigs[rig].Replace("Art/Models/", "");


            //AIS is gone pepehands

            //string aiText = File.ReadAllText(Path.Combine(basePath, monsterVariety.GetString("AISFile")));
            string aiText = "NO AI";

            HTMLWriter html = new HTMLWriter(File.Open(@"E:\A\A\Visual Studio\Archbestiary\web\Monsters\" + monsterID.TrimEnd('_').Replace('/', '_') + ".html", FileMode.Create));
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
                            HTML.Row(HTML.Cell("Accuracy:"), HTML.Cell("asdf", id: "accuracy"), HTML.Cell("Chaos Resistance:", "cellChaos"), HTML.Cell("0", id: "chaos")), //"Damage Mult:", damageMult
                            HTML.Row("Damage Mult:", damageMult, "Damage Spread", damageSpread) //"Damage Mult:", damageMult

                        ),
                        HTML.Break(),
                        grantedEffacts.CreateGrantedEffectTables(monsterVariety, onUpdate, usedFunctions, damageMult, damageSpread)
                    ),
                    HTML.Array(

                        HTML.TableClass("block",
                            HTML.Row("Id:", monsterID),
                            HTML.Row("Obj:", ListStrings(aos)),
                            HTML.Row("Rig:", ListStrings(rigs))
                        ),
                        HTML.Break(),
                        HTML.TableClass("block", CreateMonsterModRows(monsterVariety)),
                        HTML.Break(),
                        CreateMonsterStatTable(monsterVariety),
                        HTML.Break(),
                        CreateMonsterRelationTable(monsterRelations, monsterVarietyRow),
                        HTML.Break(),
                        HTML.TableClass("block", HTML.Array(monsterLocations.ContainsKey(monsterVarietyRow) ? monsterLocations[monsterVarietyRow].ToHTMLTableFixedColumns(4) : null)),
                        HTML.Break(),
                        HTML.TableClass("block", HTML.Row(monsterVariety.GetString("AISFile")),  HTML.Row(aiText)),
                        HTML.Break(),
                        HTML.TableClass("block", animationText)
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


    HashSet<int> GetMapMonsters() {
        HashSet<int> mapMonsters = new HashSet<int>();
        HashSet<int> mapPacks = new HashSet<int>();


        foreach (DatRow map in dats["Maps.dat64"])
            foreach (DatReference pack in map["MonsterPacksKeys"].GetReferenceArray())
                mapPacks.Add(pack.RowIndex);


        foreach(DatRow pack in dats["MonsterPacks.dat64"]) {
            foreach (DatReference worldArea in pack["WorldAreasKeys"].GetReferenceArray()) {
                if(worldArea.GetReferencedRow().GetID() == "3_3_8_4") {
                    mapPacks.Add(pack.rowIndex);
                    foreach (DatReference boss in pack["BossMonster_MonsterVarietiesKeys"].GetReferenceArray()) 
                        mapMonsters.Add(boss.RowIndex);
                }
            }
        }

        foreach(DatRow packEntry in dats["MonsterPackEntries.dat64"]) {
            if (mapPacks.Contains(packEntry["MonsterPacksKey"].GetReference().RowIndex)) mapMonsters.Add(packEntry["MonsterVarietiesKey"].GetReference().RowIndex);
        }


        return mapMonsters;
    }


    int[] GetMonsterCategories(HashSet<int> bosses) {

        Dictionary<int, int> areaCategories = new Dictionary<int, int>();
        Dictionary<int, int> packCategories = new Dictionary<int, int>();


        //map packs
        foreach (DatRow map in dats["Maps.dat64"])
            foreach (DatReference pack in map["MonsterPacksKeys"].GetReferenceArray())
                packCategories[pack.RowIndex] = (int)MonsterCategories.Atlas;


        int[] monsters = new int[dats["MonsterVarieties.dat64"].RowCount];
        for (int i = 0; i < monsters.Length; i++) monsters[i] = (int)MonsterCategories.Uncategorized;


        foreach (DatRow pin in dats["MapPins.dat64"])
            foreach (DatReference areaRef in pin.GetRefArray("WorldAreasKeys")) {
                int act = areaRef.GetReferencedRow().GetInt("Act") - 1;
                areaCategories[areaRef.RowIndex] = act;
            }

        foreach (DatRow map in dats["Maps.dat64"]) {
            string character = map.GetString("Regular_GuildCharacter");
            if (character == "") continue;


            areaCategories[map.GetRef("Regular_WorldAreasKey").RowIndex] = (int)MonsterCategories.Atlas;
            var unique = map.GetRef("Unique_WorldAreasKey");
            if(unique is not null) areaCategories[unique.RowIndex] = (int)MonsterCategories.Atlas;
        }


            //

            foreach (DatRow invasion in dats["InvasionMonstersPerArea.dat64"]) {
            int area = invasion.GetRef("WorldAreasKey").RowIndex;
            if(areaCategories.ContainsKey(area)) {
                foreach (DatReference monster in invasion.GetRefArray("MonsterVarietiesKeys1"))
                    AddMonsterCategory(monsters, monster.RowIndex, areaCategories[area]);
                foreach (DatReference monster in invasion.GetRefArray("MonsterVarietiesKeys2"))
                    AddMonsterCategory(monsters, monster.RowIndex, areaCategories[area]);

            }
        }
            

        
        //area 
        foreach(DatRow area in dats["WorldAreas.dat64"])
            if(areaCategories.ContainsKey(area.rowIndex)) {
                foreach(DatReference boss in area.GetRefArray("Bosses_MonsterVarietiesKeys"))
                    AddMonsterCategory(monsters, boss.RowIndex, areaCategories[area.rowIndex]);
                foreach (DatReference monster in area.GetRefArray("Monsters_MonsterVarietiesKeys")) //preloaded? includes old dweller of the deep which is yucky
                    AddMonsterCategory(monsters, monster.RowIndex, areaCategories[area.rowIndex]);

            }


        
        //monster packs
        foreach (DatRow packEntry in dats["MonsterPackEntries.dat64"]) 
            foreach(DatReference area in packEntry["MonsterPacksKey"].GetReference().GetReferencedRow()["WorldAreasKeys"].GetReferenceArray()) 
                if(areaCategories.ContainsKey(area.RowIndex)) 
                    AddMonsterCategory(monsters, packEntry["MonsterVarietiesKey"].GetReference().RowIndex, areaCategories[area.RowIndex]);
        

        //pack bosses
        foreach(DatRow pack in dats["MonsterPacks.dat64"]) 
                foreach (DatReference area in pack["WorldAreasKeys"].GetReferenceArray())
                    if (areaCategories.ContainsKey(area.RowIndex))
                        foreach (DatReference bossMonster in pack["BossMonster_MonsterVarietiesKeys"].GetReferenceArray())
                            AddMonsterCategory(monsters, bossMonster.RowIndex, areaCategories[area.RowIndex]);


        //Rogue Exiles
        foreach (DatRow exile in dats["RogueExiles.dat64"]) {
            if (exile.GetBool("NaturalSpawn")) {
                AddMonsterCategory(monsters, exile.GetRef("MonsterVarietiesKey").RowIndex, MonsterCategories.Anarchy);
                bosses.Add(exile.GetRef("MonsterVarietiesKey").RowIndex);
            }
        }

        //Tormented Spirits
        foreach (DatRow spirit in dats["TormentSpirits.dat64"]) 
            if(spirit.GetInt("SpawnWeight") > 0) {
                AddMonsterCategory(monsters, spirit.GetRef("MonsterVarietiesKey").RowIndex, MonsterCategories.Torment);
                bosses.Add(spirit.GetRef("MonsterVarietiesKey").RowIndex);
            }

        return monsters;
    }

    void AddMonsterCategory(int[] monsters, int monster, MonsterCategories category) {
        AddMonsterCategory(monsters, monster, (int)category);
    }


    void AddMonsterCategory(int[] monsters, int monster, int category) {
        if ( category < monsters[monster]) {
            monsters[monster] = category;
        }
    }

    public Dictionary<int, List<string[]>> BuildMonsterLocations() {
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



    public Dictionary<int, HashSet<(int Monster, string Type)>> BuildMonsterRelations() {
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

    string CreateMonsterStatTable(DatRow monster) {
        List<string> stats = new List<string>();
        List<string> statValues = new List<string>();
        List<string> statSources = new List<string>();


        //mods
        foreach(DatReference modRef in monster["ModsKeys"].GetReferenceArray()) {
            DatRow mod = modRef.GetReferencedRow();
            for(int i = 1; i <= 6; i++) {
                DatReference stat = mod[$"StatsKey{i}"].GetReference();
                if (stat is null) continue;
                int min = mod.GetInt($"Stat{i}Min"); int max = mod.GetInt($"Stat{i}Min");
                stats.Add(stat.GetReferencedRow().GetID());
                statValues.Add(min == max ? min.ToString() : $"{min}-{max}");
                statSources.Add(mod.GetID());
            }

        }
        

        //.ot stats
        string otPath = monster.GetString("BaseMonsterTypeIndex") + ".ot";
        ExpandMonsterOT(otPath, stats, statValues, statSources);

        string[] tableRows = new string[stats.Count];

        for(int i = 0; i < stats.Count; i++) {
            tableRows[i] = HTML.Row(stats[i], statValues[i], statSources[i]);
        }
        return HTML.TableClass("block", tableRows);

    }

    void ExpandMonsterOT(string otPath, List<string> stats, List<string> values, List<string> sources) {
        if (otPath == "Metadata/Monsters/Monster.ot") return;
        if (!monsterOTs.ContainsKey(otPath)) monsterOTs[otPath] = new ObjectTemplate(basePath, otPath);
        var ot = monsterOTs[otPath];
        foreach (string stat in ot.stats.Keys) {
            stats.Add(stat);
            values.Add(ot.stats[stat]);
            sources.Add(ot.path.Substring(9, ot.path.Length - 9));
        }
        foreach (string parent in ot.parents) ExpandMonsterOT(parent, stats, values, sources);
    }

    string[] CreateMonsterModRows(DatRow monster) {
        DatReference[] refs = monster["ModsKeys"].GetReferenceArray();
        string[] text = new string[refs.Length];
        for (int i = 0; i < refs.Length; i++) {
            text[i] = HTML.Row(refs[i].GetReferencedRow().GetID());
        }
        return text;
    }


 
        
        






    public static string GetMonsterCleanId(DatRow monsterVariety, bool replaceSlashes = true) {
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



}

