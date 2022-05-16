using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;

DatSpecIndex spec = DatSpecIndex.Create(@"E:\Extracted\PathOfExile\3.18.Sentinel\schemaformatted.json");
DatFileIndex dats = new DatFileIndex(new DiskDirectory(@"E:\Extracted\PathOfExile\3.18.Sentinel\Data\"), spec);

for(int i = 1; i < dats["MonsterVarieties.dat"].RowCount; i++) {
    var monsterVariety = dats["MonsterVarieties.dat"][i];
    DatRow monsterType = monsterVariety["MonsterTypesKey"].GetReference().GetReferencedRow();

    int fireRes = 0; int coldRes = 0; int lightningRes = 0; int chaosRes = 0;
    DatReference? resReference = monsterType["MonsterResistancesKey"].GetReference();
    if(resReference is not null) {
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


    DatReference[] grantedEffectReference = monsterVariety["GrantedEffectsKeys"].GetReferenceArray();
    List<DatRow> grantedEffects = new List<DatRow>(grantedEffectReference.Length); 
    for (int row = 0; row < grantedEffectReference.Length; row++) if(grantedEffectReference[row] != null)  grantedEffects.Add(grantedEffectReference[row].GetReferencedRow());

    string monsterID = monsterVariety["Id"].GetString();
    monsterID = monsterID.Replace("Metadata/Monsters/", "");
    string monsterName = monsterVariety["Name"].GetString();


    //Console.WriteLine(MakeLine( monsterType["Id"].GetString(), 
    //    lifeMult, ailmentMult, armourMult, evasionMult, esMult, fireRes, coldRes, lightningRes, chaosRes));


    string s = $@"
    <table>
        <tr><td colspan=""4"">{monsterName}</td></tr>
        <tr><td colspan=""4"">{monsterID}</td></tr>
        <tr><td>Life:</td><td>{lifeMult}</td><td>Ailment Threshold:</td><td>{ailmentMult}</td></tr>
        <tr><td>Armour:</td><td>{armourMult}</td><td>Fire Resistance:</td><td>{fireRes}</td></tr>
        <tr><td>Evasion:</td><td>{evasionMult}</td><td>Cold Resistance:</td><td>{coldRes}</td></tr>
        <tr><td>Energy Shield:</td><td>{esMult}</td><td>Lightning Resistance:</td><td>{lightningRes}</td></tr>
        <tr><td>Damage Multiplier:</td><td>{damageMult}</td><td>Chaos Resistance:</td><td>{chaosRes}</td></tr>
    </table>
    ";



    File.WriteAllText(@"E:\Anna\Anna\Visual Studio\Archbestiaryweb\" + monsterID.Replace('/', '_') + ".html", s);

    Console.WriteLine($"<a href=\"{monsterID.Replace('/', '_')}.html\" target=\"body\">{monsterID}</a>");

}

string MakeLine(params object[] vals) {
    StringBuilder s = new StringBuilder();
    for (int i = 0; i < vals.Length; i++) { s.Append(vals[i].ToString()); s.Append('\t'); }
    return s.ToString();
}