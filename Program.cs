using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;

Dictionary<int, DatRow> grantedEffectPerLevelsMax;

DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemaformatted.json");
DatFileIndex dats = new DatFileIndex(new DiskDirectory(@"E:\Extracted\PathOfExile\3.18.Sentinel\Data\"), spec);

CreateMonsterPages();

void CreateMonsterPages() {

    grantedEffectPerLevelsMax = BuildEffectPerLevels(dats);


    for (int i = 1; i < dats["MonsterVarieties.dat"].RowCount; i++) {
        var monsterVariety = dats["MonsterVarieties.dat"][i];
        DatRow monsterType = monsterVariety["MonsterTypesKey"].GetReference().GetReferencedRow();

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


        //Console.WriteLine(MakeLine( monsterType["Id"].GetString(), 
        //    lifeMult, ailmentMult, armourMult, evasionMult, esMult, fireRes, coldRes, lightningRes, chaosRes));

        StringBuilder html = new StringBuilder();

        html.Append($@"
    <table>
        <tr><td colspan=""4""><h4 style=""margin-bottom: 0px;"">{monsterName}</h4></td></tr>
        <tr><td colspan=""4"">{monsterID}</td></tr>
        <tr><td>Life Mult:</td><td>{lifeMult}</td><td>Ailment Threshold:</td><td>{ailmentMult}</td></tr>
        <tr><td>Armour Mult:</td><td>{armourMult}</td><td>Fire Resistance:</td><td>{fireRes}</td></tr>
        <tr><td>Evasion Mult:</td><td>{evasionMult}</td><td>Cold Resistance:</td><td>{coldRes}</td></tr>
        <tr><td>Energy Shield Mult:</td><td>{esMult}</td><td>Lightning Resistance:</td><td>{lightningRes}</td></tr>
        <tr><td>Damage Mult:</td><td>{damageMult}</td><td>Chaos Resistance:</td><td>{chaosRes}</td></tr>
    </table>
    ");

        foreach (DatReference r in monsterVariety["GrantedEffectsKeys"].GetReferenceArray()) if (r is not null) {
                DatRow grantedEffect = r.GetReferencedRow();
                DatReference rSkill = grantedEffect["ActiveSkill"].GetReference();
                DatRow activeSkill = rSkill.GetReferencedRow();
                string grantedEffectName = grantedEffect["Id"].GetString(); string skillName = activeSkill["Id"].GetString();
                DatRow grantedEffectPerLevel = grantedEffectPerLevelsMax[r.RowIndex];

                html.Append("<br/><table>");
                html.Append($"<tr><td><h4 style=\"margin-bottom: 0px;\">{grantedEffectName} ({r.RowIndex})</h4></td></tr>");
                html.Append($"<tr><td>{skillName} ({rSkill.RowIndex})</td></tr>");
                string damageType = GetSkillDamageTypes(activeSkill);
                if(damageType is not null) html.Append($"<tr><td>{damageType}</td></tr>");
                html.Append("</table><br/>");

                //foreach()
            }



        File.WriteAllText(@"E:\Anna\Anna\Visual Studio\Archbestiaryweb\" + monsterID.Replace('/', '_') + ".html", html.ToString());

        //Console.WriteLine($"<a href=\"{monsterID.Replace('/', '_')}.html\" target=\"body\">{monsterID}</a>");

    }
}



string GetSkillDamageTypes(DatRow activeSkill) {
    HashSet<int> contextFlags = new HashSet<int>();
    foreach (DatReference contextFlagRef in activeSkill["ContextFlags"].GetReferenceArray()) contextFlags.Add(contextFlagRef.RowIndex);
    StringBuilder s = new StringBuilder();

    if (contextFlags.Contains(2)) s.Append("Attack, ");
    if (contextFlags.Contains(3)) s.Append("Spell, ");
    if (contextFlags.Contains(4)) s.Append("Secondary, ");
    if (!contextFlags.Contains(18) && contextFlags.Contains(16)) {
        if (contextFlags.Contains(12)) s.Append("Spell Damage over Time, ");
        else s.Append("Damage over Time, ");
    }
    if (s.Length > 0) { s.Remove(s.Length - 2, 2); return s.ToString(); }
    return null;
}


void ListDatRowCounts() {
    foreach (DatFile dat in dats.Values) {
        if (dat.Name.EndsWith(".dat"))
            Console.WriteLine($"{dat.RowCount}|{dat.Name}");
    }
}

string MakeLine(params object[] vals) {
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