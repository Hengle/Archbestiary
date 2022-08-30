using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;
using Archbestiary.Util;
using PoeTerrain;
using System.Net.Security;

class Program {
    public static void Main(string[] args) {

        Bestiary b = new Bestiary();
        Scripts.MonsterBaseStats(b); return;
        //b.CreateMonsterList(); return;
        b.CreateMonsterPages(); return;
        /*
        for (int i = 1; i <= 100; i++) {
            float dam = (3885209 + 360246 * (i - 1)) / 1000000f;

            Console.WriteLine($"{i} - {dam}");
        }
        */




        //BLESSED SISTER CHAMBER
        //DamageDoneTest(3581, 3312, 3081, 2808, 2560, 2355, 2092, 1819, 1580, 1331);
        //DamageDoneTest(3581, 3353, 3079, 2883, 2646, 2438, 2172, 1921, 1656, 1390, 1186, 976, 755, 519, 246);
        //DamageDoneTest(3581, 3347, 3140, 2940, 2686, 2498, 2285, 2058, 1798, 1532, 1266, 1068, 834, 571, 340);

        //GALVANIC RIBBON SOLARIS 1
        DamageDoneTest(3814, 3781, 3766, 3740, 3714, 3695, 3675, 3651, 3624, 3589, 3574, 3549, 3533, 3512, 3490, 3475, 3445, 3420, 3394, 3379, 3350, 3336, 3310, 3282, 3249);
        
        for (int i = 1; i <= 100; i++) {
            Console.Write($"{i} - ");
            PrintDamage(i, 0.85, 0.03, 0.6, 1.8);
        }

    }

    static void DamageDoneTest(params int[] life) {
        for(int i = 0; i < life.Length - 1; i++) {
            Console.WriteLine(life[i] - life[i+1]);
        }
    }

    static void PrintDamage(int level, double baseEffectiveness, double incrementalEffectiveness, double min = 0.8, double max = 1.2) {
        double levelMult = (3885209 + 360246 * (level - 1)) / 1000000.0;
        double damage = levelMult * baseEffectiveness * (float)Math.Pow(1 + incrementalEffectiveness, level - 1);
        int minDam = (int)(damage * min);
        int maxDam = (int)(damage * max);
        Console.WriteLine($"Deals {minDam} to {maxDam} damage");
    }
}