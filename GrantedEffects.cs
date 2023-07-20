using Archbestiary.Util;
using PoeSharp.Filetypes.Dat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class GrantedEffects {
    DatFileIndex dats;
    Dictionary<int, List<DatRow>> grantedEffectPerLevels;
    Dictionary<int, List<DatRow>> grantedStatSetPerLevels;

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

    public GrantedEffects(DatFileIndex dats) {
        this.dats = dats;
        grantedEffectPerLevels = BuildEffectPerLevels(dats);
        grantedStatSetPerLevels = BuildStatSetPerLevels(dats);
    }

    Dictionary<int, List<DatRow>> BuildEffectPerLevels(DatFileIndex dats) {
        Dictionary<int, List<DatRow>> effectPerLevels = new Dictionary<int, List<DatRow>>();

        foreach (DatRow row in dats["GrantedEffectsPerLevel.dat64"]) {
            int level = row["PlayerLevelReq"].GetPrimitive<int>();
            int grantedEffect = row["GrantedEffect"].GetReference().RowIndex;
            if (!effectPerLevels.ContainsKey(grantedEffect)) effectPerLevels[grantedEffect] = new List<DatRow>();
            int insert = 0;
            for (int i = 0; i < effectPerLevels[grantedEffect].Count; i++) {
                int checkLevel = effectPerLevels[grantedEffect][i]["PlayerLevelReq"].GetPrimitive<int>();
                if (level == checkLevel) { //higher level gem with same level requirement, only happens on player skills
                    insert = -1;
                    effectPerLevels[grantedEffect][i] = row;
                    break;
                } else if (checkLevel < level) insert++;
            }
            if (insert >= 0) effectPerLevels[grantedEffect].Insert(insert, row);
        }



        return effectPerLevels;
    }


    Dictionary<int, List<DatRow>> BuildStatSetPerLevels(DatFileIndex dats) {
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


    public string CreateGrantedEffectTables(DatRow monsterVariety, List<string> onUpdate, HashSet<string> usedFunctions, int damageMult, int damageSpread) {
        DatReference[] refs = monsterVariety["GrantedEffectsKeys"].GetReferenceArray();
        if (refs is null) return "";
        StringBuilder effectTables = new StringBuilder();
        for (int i = 0; i < refs.Length; i++) {
            effectTables.AppendLine(CreateGrantedEffectHtml(refs[i].GetReferencedRow(), refs[i].RowIndex, onUpdate, usedFunctions, damageMult, damageSpread));
        }
        return effectTables.ToString();
    }

    public bool[] GetDamageTypes(DatRow monster) {
        bool[] damageTypes = new bool[5];
        foreach(DatReference effectRef in monster.GetRefArray("GrantedEffectsKeys")) {
            //for conversion only?
            int statSetId = effectRef.GetReferencedRow().GetRef("StatSet").RowIndex;
            DatRow perLevel = grantedStatSetPerLevels[statSetId][grantedStatSetPerLevels[statSetId].Count - 1];
            foreach(DatReference floatStat in perLevel.GetRefArray("FloatStats")) {
                string id = floatStat.GetReferencedRow().GetID();
                if (id == "spell_minimum_base_physical_damage" || id == "base_physical_damage_to_deal_per_minute") {
                    Console.WriteLine($"{monster.GetID()} | {effectRef.GetReferencedRow().GetID()} | {id}");
                    damageTypes[0] = true;
                } else if (id == "spell_minimum_base_fire_damage" || id == "base_fire_damage_to_deal_per_minute") damageTypes[1] = true;
                else if (id == "spell_minimum_base_cold_damage" || id == "base_cold_damage_to_deal_per_minute") damageTypes[2] = true;
                else if (id == "spell_minimum_base_lightning_damage" || id == "base_lightning_damage_to_deal_per_minute") damageTypes[3] = true;
                else if (id == "spell_minimum_base_chaos_damage" || id == "base_chaos_damage_to_deal_per_minute") damageTypes[4] = true;
            }
        }


        return damageTypes;
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
                    string cleanId = Bestiary.GetMonsterCleanId(monsterVariety);
                    return $"Summons <a href=\"{cleanId}.html\" target=\"body\">{monsterVariety["Name"].GetString()}</a>";
                }
            }
        }
        return $"{id} {intStatValue}";
    }


    string CreateGrantedEffectHtml(DatRow grantedEffect, int row, List<string> onUpdate, HashSet<string> usedFunctions, int damageMult = 100, int damageSpread = 20, bool debug = false) {
            float[] damageValues = new float[10];


            StringBuilder w = new StringBuilder();


            w.AppendLine("<br/><table class=\"block\">");
            w.AppendLine(HTML.Row(HTML.Cell($"<h4>{grantedEffect.GetID()} ({row})</h4>", "cellGem")));

        DatReference animation = grantedEffect.GetRef("Animation");
        if (animation is not null) w.AppendLine(HTML.Row(HTML.Cell($"{animation.GetReferencedRow().GetID()}")));
        else w.AppendLine(HTML.Row(HTML.Cell($"NO ANIMATION")));

        PriorityQueue<string, int> stats = new PriorityQueue<string, int>();

            bool isAttack = false;
            bool isHit = false;
            //ActiveSkill
            {
                if (debug) w.AppendLine(HTML.Row(HTML.Cell("ActiveSkill", "cellFire")));
                DatReference rSkill = grantedEffect["ActiveSkill"].GetReference();
                DatRow activeSkill = rSkill.GetReferencedRow();
                int skillId = rSkill.RowIndex;
                string skillName = activeSkill.GetID();
                //string damageType = GetSkillDamageTypes(activeSkill);
                //if (damageType is not null)
                //    w.AppendLine($"<tr><td>{skillName} ({skillId}) - {damageType}</td></tr>");
                //else
                w.AppendLine($"<tr><td>{skillName} ({skillId})</td></tr>");
                foreach (DatReference contextFlag in activeSkill["VirtualStatContextFlags"].GetReferenceArray()) {
                    if (contextFlag.RowIndex == 2) isAttack = true;
                    else if (contextFlag.RowIndex == 18) isHit = true;

                }

            }


            //GrantedEffectStatSets


            if (debug) w.AppendLine(HTML.Row(HTML.Cell("GrantedEffectStatSet", "cellFire")));
            DatRow statSet = grantedEffect["StatSet"].GetReference().GetReferencedRow();
            float baseEffectiveness = statSet["BaseEffectiveness"].GetPrimitive<float>();
            float incrementalEffectiveness = statSet["IncrementalEffectiveness"].GetPrimitive<float>();
            //w.AppendLine($"<tr><td>Effectiveness: {baseEffectiveness} {incrementalEffectiveness}</td></tr>");

            {
                DatReference[] constantStats = statSet["ConstantStats"].GetReferenceArray();
                int[] constantStatValues = statSet["ConstantStatsValues"].GetPrimitiveArray<int>();
                for (int stat = 0; stat < constantStats.Length; stat++) {
                    stats.Enqueue($"<tr><td>{GetStatDescription(constantStats[stat].GetReferencedRow(), constantStatValues[stat])}</td></tr>", constantStats[stat].RowIndex);
                }
                foreach (DatReference staticStatRef in statSet["ImplicitStats"].GetReferenceArray()) {
                    stats.Enqueue($"<tr><td>{staticStatRef.GetReferencedRow()["Id"].GetString()}</td></tr>", staticStatRef.RowIndex);
                    w.AppendLine();
                }
            }


            //GrantedEffectsPerLevel


            if (debug) w.AppendLine(HTML.Row(HTML.Cell("GrantedEffectsPerLevel", "cellFire")));

            var effectPerLevels = grantedEffectPerLevels[row];

            int attackSpeedMult = effectPerLevels[0].GetInt("AttackSpeedMultiplier");  //technically changes for like 3 things


            int cooldownGroup = effectPerLevels[0].GetInt("CooldownGroup");  //technically changes for like 1 thing but I think its a bug
            List<int> levelReqs = new List<int>(); levelReqs.Add(effectPerLevels[0].GetInt("PlayerLevelReq"));
            List<int> storedUses = new List<int>(); storedUses.Add(effectPerLevels[0].GetInt("StoredUses"));
            List<int> cooldowns = new List<int>(); cooldowns.Add(effectPerLevels[0].GetInt("Cooldown"));



            for (int i = 1; i < effectPerLevels.Count; i++) {
                int newLevelReq = effectPerLevels[i].GetInt("PlayerLevelReq");
                int newStoredUses = effectPerLevels[i].GetInt("StoredUses");
                int newCooldown = effectPerLevels[i].GetInt("Cooldown");
                if (newStoredUses != storedUses[storedUses.Count - 1] || newCooldown != cooldowns[cooldowns.Count - 1]) {
                    levelReqs.Add(newLevelReq); storedUses.Add(newStoredUses); cooldowns.Add(newCooldown);
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

            int baseLevelReq = levels[0].GetInt("PlayerLevelReq"); //Do we need this for anything? I guess interpolation 2

            List<int> spellCritLevels = new List<int>() { baseLevelReq };
            List<int> spellCritValues = new List<int>() { levels[0].GetInt("SpellCritChance") };

            List<int> baseMultiplierLevels = new List<int>() { baseLevelReq };
            List<int> baseMultiplierValues = new List<int>() { levels[0].GetInt("BaseMultiplier") };

            List<int> additionalFlagLevels = new List<int>() { baseLevelReq };
            List<string> additionalFlags = new List<string>() { levels[0].GetReferenceArrayIDsFormatted("AdditionalFlags") };




            Dictionary<int, List<int>> intStatLevels = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> intStatValues = new Dictionary<int, List<int>>();

            {
                var intStats = levels[0]["AdditionalStats"].GetReferenceArray();
                var intValues = levels[0]["AdditionalStatsValues"].GetPrimitiveArray<int>();
                for (int i = 0; i < intStats.Length; i++) {
                    intStatLevels[intStats[i].RowIndex] = new List<int> { baseLevelReq };
                    intStatValues[intStats[i].RowIndex] = new List<int> { intValues[i] };
                }

            }


            Dictionary<int, List<int>> floatStatLevels = new Dictionary<int, List<int>>();
            Dictionary<int, List<float>> floatStatValues = new Dictionary<int, List<float>>();
            {
                var floatStats = levels[0]["FloatStats"].GetReferenceArray();
                var floatValues = levels[0]["FloatStatsValues"].GetPrimitiveArray<float>();
                for (int i = 0; i < floatStats.Length; i++) {
                    floatStatLevels[floatStats[i].RowIndex] = new List<int> { baseLevelReq };
                    floatStatValues[floatStats[i].RowIndex] = new List<float> { floatValues[i] };
                }

            }

            //string test = "BaseMultiplier";
            for (int i = 1; i < levels.Count; i++) {
                int level = levels[i].GetInt("PlayerLevelReq");

                int newCrit = levels[i].GetInt("SpellCritChance");
                if (newCrit != spellCritValues[spellCritValues.Count - 1]) {
                    spellCritLevels.Add(level);
                    spellCritValues.Add(newCrit);
                }

                int newBaseMultiplier = levels[i].GetInt("BaseMultiplier");
                if (newBaseMultiplier != baseMultiplierValues[baseMultiplierValues.Count - 1]) {
                    baseMultiplierLevels.Add(level);
                    baseMultiplierValues.Add(newBaseMultiplier);
                }

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
                    if (!floatStatLevels.ContainsKey(statIndex)) Console.WriteLine("FLOAT STAT ADDED, THIS SHOULD NEVER HAPPEN");
                    else if (floatStatValues[statIndex][floatStatValues[statIndex].Count - 1] != floatValues[stat]) {
                        floatStatLevels[statIndex].Add(level);
                        floatStatValues[statIndex].Add(floatValues[stat]);
                    }
                }
            }


            //Cooldown
            for (int i = 0; i < storedUses.Count; i++)
                if (storedUses[i] != 0) {
                    if (levelReqs.Count > 1) {
                        w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_c")));
                        //TODO FORMAT LEVELED COUNTDOWN
                        onUpdate.Add(@$"		SetCooldown(""{row}_c"", slider.value, {HTML.JSArray(levelReqs.ToArray())}, {HTML.JSArray(storedUses.ToArray())}, {HTML.JSArray(cooldowns.ToArray())});");
                        usedFunctions.Add("SetCooldown");
                    } else {
                        if (storedUses[0] > 1) w.AppendLine(HTML.Row(HTML.Cell($"<span class=\"statTag\">Cooldown Time:</span> {((float)cooldowns[0]) / 1000} sec ({storedUses[0]} uses)")));
                        else w.AppendLine(HTML.Row(HTML.Cell($"<span class=\"statTag\">Cooldown Time:</span> {((float)cooldowns[0]) / 1000} sec")));
                    }
                    break;
                }

            //Cast Time
            if (!isAttack) w.AppendLine(HTML.Row(HTML.Cell(string.Format("<span class=\"statTag\">Cast Time:</span> {0:F2} sec", ((float)grantedEffect["CastTime"].GetPrimitive<int>()) / 1000))));


            //Spell crit
            if (!isAttack && isHit) {
                if (spellCritValues.Count > 1) {
                    w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_s")));
                    //TODO FORMAT LEVELED SPELL CRIT
                    onUpdate.Add(@$"		SetIntStat(""{row}_s"", slider.value, ""Spell Crit Chance: "", {HTML.JSArray(spellCritLevels.ToArray())}, {HTML.JSArray(spellCritValues.ToArray())});");
                } else {
                    w.AppendLine(HTML.Row(HTML.Cell(string.Format("<span class=\"statTag\">Critical Strike Chance:</span> {0:F2}%", ((float)spellCritValues[0]) / 100))));
                }
            }

            //Attack Mult
            if (isAttack) {
                if (baseMultiplierValues.Count > 1) {
                    w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_m")));
                    //TODO FORMAT LEVELED ATTACK MULT
                    onUpdate.Add(@$"		SetIntStat(""{row}_m"", slider.value, ""<span class=\""statTag\"">Attack Damage:</span> "", {HTML.JSArray(baseMultiplierLevels.ToArray())}, {HTML.JSArray(baseMultiplierValues.ToArray())});");
                } else {
                    w.AppendLine(HTML.Row(HTML.Cell($"<span class=\"statTag\">Attack Damage:</span> {((float)(baseMultiplierValues[0] / 10)) / 10 + 100}% of Base")));
                }
            }

            //Attack Speed
            if (isAttack) w.AppendLine(HTML.RowList("<span class=\"statTag\">Attack Speed :</span> " + (100 + attackSpeedMult).ToString() + "% of Base"));

            w.AppendLine(HTML.Row());

            foreach (int statRow in intStatLevels.Keys) {
                stats.Enqueue(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_{statRow}")), statRow);
                //w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_{statRow}")));
                onUpdate.Add(@$"		SetIntStat(""{row}_{statRow}"", slider.value, ""{dats["Stats.dat64"][statRow].GetID()}"", {HTML.JSArray(intStatLevels[statRow].ToArray())}, {HTML.JSArray(intStatValues[statRow].ToArray())});");
                usedFunctions.Add("SetIntStat");
            }

            foreach (int statRow in floatStatLevels.Keys) {
                stats.Enqueue(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_{statRow}")), statRow);
                //w.AppendLine(HTML.Row(HTML.Cell("C", "statDamage", $"{row}_{statRow}")));
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

        while (stats.Count > 0) {
            w.AppendLine(stats.Dequeue());
        }


        w.Append("</table>");
        return w.ToString();
    }
}
