using Archbestiary.Util;
using ImageMagick;
using PoeSharp.Filetypes.Dat;
using PoeFormats;
using System;
using System.Text;
using System.IO;
using ImageMagick.Formats;

static class Scripts {

    public static void ListLife(Bestiary b) {
        foreach(DatRow row in b.dats["DefaultMonsterStats.dat64"]) {
            string level = row.GetString("DisplayLevel");
            int life1 = row.GetInt("Life");
            int life2 = row.GetInt("UnkLife1");
            int life3 = row.GetInt("UnkLife2");
            Console.WriteLine($"{level}|{life1}|{life2}|{life3}");
        }
    }

    public static void ListPacks(Bestiary b) {
        Dictionary<string, List<string>> packs = new Dictionary<string, List<string>>();

        foreach (DatRow row in b.dats["MonsterPacks.dat64"]) {
            string packName = row["Id"].GetString();
            packs[packName] = new List<string>();
        }
        foreach (DatRow row in b.dats["MonsterPackEntries.dat64"]) {
            DatRow monster = row["MonsterVarietiesKey"].GetReference().GetReferencedRow();
            DatRow pack = row["MonsterPacksKey"].GetReference().GetReferencedRow();
            string packName = pack["Id"].GetString();
            packs[packName].Add($"{monster["Name"].GetString()}@{monster["Id"]}");
        }
        foreach (DatRow row in b.dats["MonsterPacks.dat64"]) {
            string packName = row["Id"].GetString();
            foreach (DatReference monsterRef in row["BossMonster_MonsterVarietiesKeys"].GetReferenceArray()) {
                DatRow monster = monsterRef.GetReferencedRow();
                packs[packName].Add($"{monster["Name"].GetString()}@{monster["Id"]}");
            }
        }


        foreach (string pack in packs.Keys) {
            foreach (string monster in packs[pack]) {
                Console.WriteLine(pack + "@" + monster);
            }
            Console.WriteLine();
        }
    }


    public static void FindGrantedEffectsWithoutAnimation(Bestiary b, string activeSkill) {
        foreach (DatRow monster in b.dats["MonsterVarieties.dat64"]) {
            foreach (DatReference geRef in monster.GetRefArray("GrantedEffectsKeys")) {
                DatRow grantedEffect = geRef.GetReferencedRow();
                string skillName = grantedEffect.GetRef("ActiveSkill").GetReferencedRow().GetID();
                if (skillName != activeSkill) continue;
                DatReference animationRef = grantedEffect.GetRef("Animation");
                if(animationRef is null) {
                    Console.WriteLine($"{monster.GetID()} {grantedEffect.GetID()} {activeSkill}");
                }
            }
        }
    }

    public static void ActiveSkillCounts(Bestiary b) {
        Dictionary<string, int> skills = new Dictionary<string, int>();
        Dictionary<string, int> skillAnimationCounts = new Dictionary<string, int>();
        Dictionary<string, HashSet<string>> skillAnimations = new Dictionary<string, HashSet<string>>();
 
        foreach (DatRow monster in b.dats["MonsterVarieties.dat64"]) {
            foreach(DatReference geRef in monster.GetRefArray("GrantedEffectsKeys")) {
                DatRow grantedEffect = geRef.GetReferencedRow();
                string skillName = grantedEffect.GetRef("ActiveSkill").GetReferencedRow().GetID();
                if(!skills.ContainsKey(skillName)) skills[skillName] = 0;
                skills[skillName] = skills[skillName] + 1;

                DatReference animationRef = grantedEffect.GetRef("Animation");
                if(animationRef is not null) {
                    string animation = animationRef.GetReferencedRow().GetID();
                    if (!skillAnimationCounts.ContainsKey(skillName)) {
                        skillAnimationCounts[skillName] = 0;
                        skillAnimations[skillName] = new HashSet<string>();
                    }
                    skillAnimationCounts[skillName] = skillAnimationCounts[skillName] + 1;
                    skillAnimations[skillName].Add(animation);
                }
            }
        }

        foreach(string skill in skills.Keys) {
            if(skillAnimationCounts.ContainsKey(skill)) {
                Console.Write($"{skills[skill]}|{skill}|{skillAnimationCounts[skill]/(float)skills[skill]}");
                foreach (string animation in skillAnimations[skill]) Console.Write("|" + animation);
                Console.WriteLine();
            } else {
                Console.WriteLine($"{skills[skill]}|{skill}");
            }
        }
    }

    public static void CreateMonsterIdleAnimations(string folder = @"F:\Anna\Desktop\test") {
        Dictionary<string, HashSet<int>> images = new Dictionary<string, HashSet<int>>();
        foreach(string path in Directory.EnumerateFiles(folder, "*.png")) {
            string filename = Path.GetFileNameWithoutExtension(path);
            string monster = filename.Substring(0, filename.LastIndexOf('_'));
            int frame = int.Parse(filename.Substring(filename.LastIndexOf('_') + 1));
            if(!images.ContainsKey(monster)) images[monster] = new HashSet<int>();
            images[monster].Add(frame);
        }

        foreach(string monster in images.Keys) {
            MagickImageCollection animation = new MagickImageCollection();
            for(int i = 1; i <= images[monster].Count; i++) {
                MagickImage image = new MagickImage($"{folder}\\{monster}_{i}.png");
                image.AnimationDelay = 3;
                animation.Add(image);
            }
            
            WebPWriteDefines defines = new WebPWriteDefines() { Lossless = true };
            Console.WriteLine(monster);
            animation.Write($"{folder}\\{monster}.webp", defines);
        }
    }

    public static void PrintMonsterRenderingInfo(Bestiary b) {
        using(TextWriter w = new StreamWriter(File.Open("monsterart.txt", FileMode.Create))) {
            for (int i = 1; i < b.dats["MonsterVarieties.dat64"].RowCount; i++) {
                DatRow monster = b.dats["MonsterVarieties.dat64"][i];
                string id = monster.GetID();
                string act = monster["ACTFiles"].GetStringArray()[0];
                string aoc = monster["AOFiles"].GetStringArray()[0] + 'c';
                string name = monster.GetName();
                w.WriteLine($"{i}@{id}@{name}@{act}@{aoc}");
            }
        }
    }

    public static void PrintBoxImage(this Mtp mtp) {
        MagickImage image = new MagickImage(MagickColors.White, 4096, 4096);
        foreach (MinimapImage i in mtp.images) {
            string writeVal = $"{i.filename}\n{i.orientation}\n{i.unk2} {i.unk3} {i.unk4}\n{i.height}\n{i.width}\n{i.leftPadding}";
            image.Draw(new Drawables()
                .FillColor(MagickColors.Transparent)
                .StrokeColor(MagickColors.GreenYellow)
                .StrokeWidth(2)
                .Rectangle(i.originX - i.leftPadding, i.originY - i.topPadding, i.originX - i.leftPadding + i.height, i.originY - i.topPadding + i.height));
        }
        image.Write(Path.GetFileNameWithoutExtension(mtp.path) + ".png");
    }

    public static void PrintValueImage(this Mtp mtp) {
        MagickImage image = new MagickImage(MagickColors.White, 4096, 4096);
        foreach (MinimapImage i in mtp.images) {
            string writeVal = $"{i.filename}\n{i.orientation}\n{i.unk2} {i.unk3} {i.unk4}\n{i.height}\n{i.width}\n{i.leftPadding}";
            image.Draw(new Drawables().FillColor(MagickColors.YellowGreen).FontPointSize(16).Gravity(Gravity.Southwest).TextAlignment(TextAlignment.Center).Text(i.originX * 2, i.originY * 2, writeVal));
        }
        image.Write(Path.GetFileNameWithoutExtension(mtp.path) + ".png");
    }

    public static void WriteImages(this Mtp mtp, string outputFolder) {
        MagickImage image = File.Exists(mtp.path.Replace(".mtp", ".png")) ? new MagickImage(mtp.path.Replace(".mtp", ".png")) : new MagickImage(mtp.path.Replace(".mtp", ".dds"));
        foreach (MinimapImage i in mtp.images) {
            MagickImage i2 = new MagickImage(image);
            i2.Crop(new MagickGeometry((int)(i.originX - i.leftPadding), (int)(i.originY - i.topPadding), i.width * 40, i.height));
            i2.RePage();
            string filename = $"{i.filename.Substring(0, i.filename.Length - 4)}_{i.orientation}.png";
            string filePath = Path.Combine(outputFolder, filename);
            if (!Directory.Exists(Path.GetDirectoryName(filePath))) Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            i2.Write(filePath);
        }
    }


    public static void ListAstAnimations(string folder) {
        using(TextWriter writer = new  StreamWriter(File.Create("ast_animations.txt"))) {
            foreach (string path in Directory.EnumerateFiles(folder, "*.ast", SearchOption.AllDirectories)) {

                //if (!path.Contains("GenericBiped")) continue;

                Ast ast = new Ast(path);

                //bool flicker = false;
                //for (int i = 0; i < ast.animations.Length; i++) if (ast.animations[i].name == "multi_beam_aoe_01") { flicker = true; break; }
                //bool lightning = false;
                //for (int i = 0; i < ast.animations.Length; i++) if (ast.animations[i].name == "spinning_slam_01") { lightning = true; break; }

                //if (!flicker || !lightning) continue;

                //if (ast.lights.Length == 0) continue;

                writer.WriteLine(path.Substring(folder.Length));

                //for (int i = 0; i < ast.lights.Length; i++) Console.WriteLine("    LIGHT - " + ast.lights[i].name);

                for (int i = 0; i < ast.animations.Length; i++) writer.WriteLine("    " + ast.animations[i].name);
                writer.WriteLine();
            }

        }
    }

    public static void ListAstVersions(string folder) {
        Random r = new Random(1);
        Dictionary<int, List<string>> astVersions = new Dictionary<int, List<string>>();
        int filesProcessed = 0;
        foreach (string path in Directory.EnumerateFiles(folder, "*.ast", SearchOption.AllDirectories)) {
            using(BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
                byte version = reader.ReadByte();
                if(!astVersions.ContainsKey(version)) astVersions[version] = new List<string>();
                astVersions[version].Add(path.Substring(folder.Length));
                filesProcessed++;
                if (filesProcessed % 100 == 0) Console.WriteLine(filesProcessed);
            }
        }
        Console.WriteLine();
        foreach (int version in astVersions.Keys) {
            foreach(string monster in astVersions[version]) {
                Console.WriteLine($"{version} {monster}");
            }

            /*
            Console.Write(version);
            //fisher-yates randomize
            for (int count = astVersions[version].Count - 1; count > 0; count--) {
                int swapVal = r.Next(count);
                string temp = astVersions[version][count];
                astVersions[version][count] = astVersions[version][swapVal];
                astVersions[version][swapVal] = temp;
            }
            for (int i = 0; i < Math.Min(astVersions[version].Count, 10); i++) Console.Write(" " + astVersions[version][i]);
            Console.WriteLine();
            */
        }
    }

    public static void ListArmVersions(string folder) {
        HashSet<string> versions = new HashSet<string>();
        int i = 0;
        using (TextWriter writer = new StreamWriter(File.Create("armversions.txt"))) {
            int offset = folder.Length + 17;
            foreach (string path in Directory.EnumerateFiles(folder, "*.arm", SearchOption.AllDirectories)) {
                using (TextReader reader = new StreamReader(File.OpenRead(path))) {
                    string version = reader.ReadLine().Substring(8);
                    if(!versions.Contains(version)) {
                        Console.WriteLine($"version {version} found");
                        versions.Add(version);
                    }
                    writer.WriteLine(version + "|" + path.Substring(offset));
                    i++;
                    if (i % 100 == 0) Console.WriteLine(i);
                }
            }
        }
    }

    public static void ListDatRowIds(Bestiary b, int row) {
        foreach(string dat in b.dats.Keys) {
            if (b.dats[dat].RowCount <= row) continue;
            if (!dat.EndsWith(".dat64")) continue;
            foreach(var col in b.dats[dat].Spec.Columns) {
                if(col.Name == "Id" && col.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.String && col.Array == false) {
                    Console.WriteLine(Path.GetFileNameWithoutExtension(dat) + " | " + b.dats[dat][row]["Id"].GetString());
                }
            }
        }
    }

    public static void ListMonsterLocations(Bestiary b) {
        using(TextWriter w = new StreamWriter(File.Open("monsterlocations.txt", FileMode.Create))) {
            var monsterLocations = b.BuildMonsterLocations();
            var monsterRelations = b.BuildMonsterRelations();
            foreach (DatRow monsterVariety in b.dats["MonsterVarieties.dat64"]) {

                //DatRow monsterType = monsterVariety["MonsterTypesKey"].GetReference().GetReferencedRow();
                //if (monsterType["IsSummoned"].GetPrimitive<bool>()) continue;

                string id = monsterVariety.GetID();
                string name = monsterVariety.GetName();
                if (name.Length >= 35) name = name.Substring(0, 35);
                w.Write(id + "@" + name);


                if (monsterLocations.ContainsKey(monsterVariety.rowIndex)) {
                    foreach (var location in monsterLocations[monsterVariety.rowIndex]) {
                        w.Write("@" + location[0]);
                        for (int v = 1; v < location.Length; v++)
                            w.Write(" - " + location[v]);
                    }
                }

                if (monsterRelations.ContainsKey(monsterVariety.rowIndex))
                    foreach (var tuple in monsterRelations[monsterVariety.rowIndex]) {
                        if (tuple.Type != "Base" && tuple.Type != "Summoned by") continue;
                        DatRow monster = b.dats["MonsterVarieties.dat64"][tuple.Monster];
                        w.Write($"@{tuple.Type} - {Bestiary.GetMonsterCleanId(monster, false)} ({monster["Name"].GetString()})");
                    }
                w.WriteLine();
            }

        }
    }


    public static void MonsterTypeList(Bestiary b) {
        Dictionary<int, List<string>> monsterVarieties = new Dictionary<int, List<string>>();
        Dictionary<int, List<string>> monsterNames = new Dictionary<int, List<string>>();

        Dictionary<int, string> monsterRigs = new Dictionary<int, string>();
        foreach(DatRow variety in b.dats["MonsterVarieties.dat64"]) {
            int monsterType = variety["MonsterTypesKey"].GetReference().RowIndex;
            string[] aos = variety["AOFiles"].GetStringArray();
            if (aos.Length > 0) {
                string rig = Bestiary.GetRigFromAO(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT", aos[0]);
                if (!monsterRigs.ContainsKey(monsterType)) monsterRigs[monsterType] = rig;
                else if (rig != monsterRigs[monsterType]) Console.WriteLine($"{monsterType} RIG MISMATCH {rig} {monsterRigs[monsterType]}");
            }
            string id = variety.GetID().TrimEnd('_');
            if (!monsterVarieties.ContainsKey(monsterType)) {
                monsterVarieties[monsterType] = new List<string>();
                monsterNames[monsterType] = new List<string>();
            }
            monsterVarieties[monsterType].Add(id);
            monsterNames[monsterType].Add(variety.GetName());
        }

        foreach(DatRow monsterType in b.dats["MonsterTypes.dat64"]) {
            int i = monsterType.rowIndex;
            StringBuilder line = new StringBuilder(monsterType.GetID()); line.Append('|');
            line.Append(monsterRigs.ContainsKey(i) ? monsterRigs[i] : "Unk"); line.Append('|');
            if (monsterVarieties.ContainsKey(i)) {
                if (monsterVarieties[i].Count <= 4) for (int variety = 0; variety < monsterVarieties[i].Count; variety++) {
                    line.Append(monsterNames[i][variety]); line.Append('|');
                    line.Append(monsterVarieties[i][variety]); line.Append('|');
                } else {
                    int skip = (monsterVarieties[i].Count / 4);
                    for (int variety = 0; variety < 4; variety++) {
                        line.Append(monsterNames[i][variety * skip]); line.Append('|');
                        line.Append(monsterVarieties[i][variety * skip]); line.Append('|'); 
                    }
                }
            }

            Console.WriteLine(line.ToString());
        }

    }

    public static void DumpGeometrySkills(Bestiary b) {

        StringBuilder s = new StringBuilder("GrantedEffect|idx|");
        foreach (var column in b.dats["GeometryAttack.dat64"].Spec.Columns) s.Append(column.Name + "|");
        Console.WriteLine(s.ToString());


        Dictionary<int, DatRow> geometryAttacks = new Dictionary<int, DatRow>();
        for (int i = 0; i < b.dats["GeometryAttack.dat64"].RowCount; i++) {
            DatRow geom = b.dats["GeometryAttack.dat64"][i];
            geometryAttacks[geom.GetInt("Id")] = geom;
        }



        HashSet<string> geometrySkillNames = new HashSet<string>(new string[] { "geometry_spell", "geometry_attack", "geometry_spell_channelled", "geometry_attack_channelled" });


        for (int i = 0; i < b.dats["GrantedEffects.dat64"].RowCount; i++) {
            DatRow effect = b.dats["GrantedEffects.dat64"][i];

            DatReference skill = effect["ActiveSkill"].GetReference(); if (skill is null) continue;
            string skillId = skill.GetReferencedRow().GetID();
            int variation = effect.GetInt("Variation");

            if (geometrySkillNames.Contains(skillId) && geometryAttacks.ContainsKey(variation)) 
                Console.WriteLine($"{effect.GetID()}|{geometryAttacks[variation].Dump()}");
        }
    }

    public static void DumpGeometryTriggers(Bestiary b) {
        using(TextWriter w = new StreamWriter(File.Open("geometrytriggers.txt", FileMode.Create))) {
            StringBuilder s = new StringBuilder("GrantedEffect|idx|");
            foreach (var column in b.dats["GeometryTrigger.dat64"].Spec.Columns) s.Append(column.Name + "|");
            w.WriteLine(s.ToString());


            Dictionary<int, DatRow> triggers = new Dictionary<int, DatRow>();
            for (int i = 0; i < b.dats["GeometryTrigger.dat64"].RowCount; i++) {
                DatRow geom = b.dats["GeometryTrigger.dat64"][i];
                triggers[geom.GetInt("Id")] = geom;
            }





            for (int i = 0; i < b.dats["GrantedEffects.dat64"].RowCount; i++) {
                DatRow effect = b.dats["GrantedEffects.dat64"][i];

                DatReference skill = effect["ActiveSkill"].GetReference(); if (skill is null) continue;
                string skillId = skill.GetReferencedRow().GetID();
                int variation = effect.GetInt("Variation");

                if (skillId == "geometry_trigger" && triggers.ContainsKey(variation))
                    w.WriteLine($"{effect.GetID()}|{triggers[variation].Dump()}");
            }

        }

    }



    public static void DumpDat(Bestiary b, string filename) {
        StringBuilder s = new StringBuilder("idx|");
        foreach (var column in b.dats[filename].Spec.Columns) s.Append(column.Name + "|");
        Console.WriteLine(s.ToString());
        for (int i = 0; i < b.dats[filename].RowCount; i++) Console.WriteLine(b.dats[filename][i].Dump());
    }

    public static void AOEffectDrivenEvents() {
        foreach(string path in Directory.EnumerateFiles(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\Metadata", "*.ao", SearchOption.AllDirectories)) {
            string text = File.ReadAllText(path);
            int e = text.IndexOf("EffectDrivenEvent");
            if (e != -1) {
                Console.WriteLine(path);
                Console.WriteLine(text.Substring(e, text.Length - e));
                //ObjectTemplate.DumpTokens(path);
                Console.WriteLine("\r\n\r\n\r\n");
            }
        }
    }

    public static void GrantedEffectMonsterSkillShape(Bestiary b) {
        Dictionary<int, int> geometryAttackShapes = new Dictionary<int, int>();
        for (int i = 0; i < b.dats["GeometryAttack.dat64"].RowCount; i++) {
            DatRow geom = b.dats["GeometryAttack.dat64"][i];
            geometryAttackShapes[geom.GetInt("Id")] = geom.GetInt("MonsterSkillsShape");
        }

        Dictionary<int, int> geometryTriggerShapes = new Dictionary<int, int>();
        for (int i = 0; i < b.dats["GeometryTrigger.dat64"].RowCount; i++) {
            DatRow geom = b.dats["GeometryTrigger.dat64"][i];
            geometryTriggerShapes[geom.GetInt("Id")] = geom.GetInt("MonsterSkillsShape");
        }

        HashSet<string> geometrySkillNames = new HashSet<string>(new string[] { "geometry_spell", "geometry_attack", "geometry_spell_channelled", "geometry_attack_channelled" });

        for (int i = 0; i < b.dats["GrantedEffects.dat64"].RowCount; i++) {
            DatRow effect = b.dats["GrantedEffects.dat64"][i];

            DatReference skill = effect["ActiveSkill"].GetReference(); if (skill is null) continue;
            string skillId = skill.GetReferencedRow().GetID();
            int variation = effect.GetInt("Variation");

            if (geometrySkillNames.Contains(skillId)) {
                if (geometryAttackShapes.ContainsKey(variation)) Console.WriteLine($"{variation}|{skillId}|{effect.GetID()}|{geometryAttackShapes[variation]}");
                else Console.WriteLine($"{variation}|{skillId}|{effect.GetID()}|MISSING GEOMETRYATTACK");
            } else if(skillId == "geometry_trigger") {
                if (geometryTriggerShapes.ContainsKey(variation)) Console.WriteLine($"{variation}|{skillId}|{effect.GetID()}|{geometryTriggerShapes[variation]}");
                else Console.WriteLine($"{variation}|{skillId}|{effect.GetID()}|MISSING GEOMETRYTRIGGER");

            }


        }
    }

    public static void GrantedEffectExtraData(Bestiary b, params string[] activeSkills) {

        if (activeSkills.Length == 0) activeSkills = new string[] { 
            "geometry_spell", "geometry_attack", "geometry_spell_channelled", "geometry_attack_channelled", 
            "geometry_trigger", "monster_mortar", "monster_mortar_attack", "execute_geal", "spawn_object", 
            "move_daemon", "geometry_projectiles_spell", "execute_geal_no_los", "geometry_mortars_spell", 
            "geometry_projectiles_attack", "effect_driven_spell", "effect_driven_attack", 
            "geometry_projectiles_attack_channelled", "add_buff_to_target_triggered", "add_buff_to_target", 
            "suicide_explosion", "trigger_beam", "expanding_pulse", "execute_corpse_geal", "single_ground_laser" 
        };

        HashSet<string> activeSkillSet = new HashSet<string>(activeSkills);

        for (int i = 0; i < b.dats["GrantedEffects.dat64"].RowCount; i++) {
            DatRow effect = b.dats["GrantedEffects.dat64"][i];

            DatReference skill = effect["ActiveSkill"].GetReference(); if (skill is null) continue;
            string id = skill.GetReferencedRow().GetID(); if (!activeSkillSet.Contains(id)) continue;

            int variation = effect.GetInt("Variation");
            Console.WriteLine($"{id}|{variation}|{effect.GetID()}");
        }

    }

    public static void ActiveSkillBehaviorIndices(Bestiary b) {
        Dictionary<string, int> skills = new Dictionary<string, int>();





        for (int i = 0; i < b.dats["GrantedEffects.dat64"].RowCount; i++) {
            DatRow effect = b.dats["GrantedEffects.dat64"][i];

            DatReference skill = effect["ActiveSkill"].GetReference(); if (skill is null) continue;

            string id = skill.GetReferencedRow().GetID();
            int idx = effect.GetInt("Variation");
            if (idx > 0 && (!skills.ContainsKey(id) || idx > skills[id])) skills[id] = idx;
        }

        foreach(string skill in skills.Keys) Console.WriteLine(skill + "|" + skills[skill].ToString());
    }
    public static void OTParent(string basePath, string path) {
        OTExpand(basePath, path, 0);
    }

    static void OTExpand(string basePath, string path, int indent) {
        ObjectTemplate ot = new ObjectTemplate(basePath, path);
        string indentString = new string(' ', indent);
        Console.WriteLine(indentString + ot.path);
        foreach (string stat in ot.stats.Keys) Console.WriteLine(indentString + stat + ": " + ot.stats[stat]);
        Console.WriteLine();
        foreach (string parent in ot.parents) OTExpand(basePath, parent, indent + 1);

    }




    public static void SkillContextFlags(Bestiary b) {
        int contextFlagCount = b.dats["VirtualStatContextFlags.dat64"].RowCount;
        Console.Write("|");
        for (int i = 0; i < contextFlagCount; i++) {
            Console.Write(b.dats["VirtualStatContextFlags.dat64"][i].GetID() + "|");
        }
        Console.WriteLine();
        for (int i = 0; i < b.dats["ActiveSkills.dat64"].RowCount; i++) {
            DatRow skill = b.dats["ActiveSkills.dat64"][i];
            HashSet<int> flags = new HashSet<int>();
            foreach (DatReference flag in skill["VirtualStatContextFlags"].GetReferenceArray()) flags.Add(flag.RowIndex);

            Console.Write($"{i}|{skill.GetID()}|");
            for (int a = 0; a < contextFlagCount; a++) Console.Write(flags.Contains(a) ? "e|" : "|");
            Console.WriteLine();
        }
    }

    public static void TestGrantedEffectStatInterpolations(Bestiary b) {
        for (int i = 0; i < b.dats["GrantedEffectStatSetsPerLevel.dat64"].RowCount; i++) {
            DatRow row = b.dats["GrantedEffectStatSetsPerLevel.dat64"][i];
            int floatCount = row["FloatStats"].GetStringArray().Length;
            int[] interpolations = row["StatInterpolations"].GetPrimitiveArray<int>();
            for (int stat = 0; stat < floatCount; stat++) if (interpolations[stat] != 3) Console.WriteLine($"FLOAT STAT {interpolations[stat]}");
            for (int stat = floatCount; stat < interpolations.Length; stat++) if (interpolations[stat] > 2) Console.WriteLine($"INT STAT {interpolations[stat]}");
        }
        return;
    }

    public static void ListDatRowCounts(Bestiary b) {
        foreach (DatFile dat in b.dats.Values) {
            if(dat.Name.Contains("Abyss")) Console.WriteLine(dat.Name);
            continue;
            if (dat.Name.EndsWith(".dat64"))
                Console.WriteLine($"{dat.RowCount}|{dat.Name}");
        }
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

    public static void ListMonsterRigsNew(Bestiary b) {
        //var history = History.BuildMonsterVarietyHistory();

        for (int i = 0; i < b.dats["MonsterVarieties.dat64"].RowCount; i++) {
            DatRow monster = b.dats["MonsterVarieties.dat64"][i];
            DatReference monsterType = monster["MonsterTypesKey"].GetReference();
            string monsterTypeId = monsterType.GetReferencedRow()["Id"].GetString();
            bool summoned = monsterType.GetReferencedRow()["IsSummoned"].GetPrimitive<bool>();
            string monsterId = monster["Id"].GetString().TrimEnd('_');
            string monsterName = monster["Name"].GetString();
            //string version = history.ContainsKey(monsterId) ? history[monsterId] : "unk";


            foreach (string ao in monster["AOFiles"].GetStringArray()) {
                string rig = Bestiary.GetRigFromAO(@"F:\Extracted\PathOfExile\3.23.Affliction", ao);
                Console.WriteLine($"{monsterType.RowIndex}@{summoned}@{monsterTypeId}@{i}@{monsterId}@{monsterName}@{ao}@{rig}");
                break;
            }
        }
    }

    public static void ListUnusedRigs() {
        HashSet<string> rigs = new HashSet<string>(Directory.EnumerateFiles(@"E:\Extracted\PathOfExile\3.21.Crucible\Art\Models", "*.amd", SearchOption.AllDirectories));
        Console.WriteLine("RIG LIST CREATED");
        foreach (string ao in Directory.EnumerateFiles(@"E:\Extracted\PathOfExile\3.21.Crucible\Metadata\", "*.ao", SearchOption.AllDirectories)) {
            string rig = Bestiary.GetRigFromAO("", ao);
            if (!rig.StartsWith("COULD NOT")) {
                string combined = Path.Combine(@"E:\Extracted\PathOfExile\3.21.Crucible\", rig).Replace('/', '\\');
                if (rigs.Contains(combined)) {
                    //Console.WriteLine(combined);
                    rigs.Remove(combined);
                }
            }
        }
        Console.WriteLine("\r\n\r\n\r\n\r\n\r\nUNUSED");
        foreach (string rig in rigs) Console.WriteLine(rig);
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
            Console.Write(row["EvasiveEvasion"].GetPrimitive<int>().ToString() + ",");
        }
        Console.Write("];\n    const accuracyPerLevel = [");
        for (int i = 0; i < b.dats["DefaultMonsterStats.dat64"].RowCount; i++) {
            DatRow row = b.dats["DefaultMonsterStats.dat64"][i];
            Console.Write(row.GetInt("Accuracy").ToString() + ",");
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