using PoeSharp.Filetypes.BuildingBlocks;
using PoeSharp.Filetypes.Dat;
using PoeSharp.Filetypes.Dat.Specification;
using System.Text;
using Archbestiary.Util;
using PoeTerrain;
using System.Net.Security;

class Program {
    public static void Main(string[] args) {

        Scripts.ListArmVersions(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\");
        //Arm room = new Arm(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT\Metadata\Terrain\Leagues\Sanctum\Nave\Rooms\encounter_lair_1_1.arm");
        return;

        //History.BuildMonsterVarietyHistory();

        //History.DatHistory(); return;

        //Scripts.OTParent(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT", @"Metadata\Monsters\LeagueSanctum\Boss\GargoyleBoss.ot"); return;

        //Scripts.AOEffectDrivenEvents(); return;

         
        Bestiary b = new Bestiary();
        b.CreateMonsterListNew(); return;


        Scripts.ListDatRowIds(b, 9); return;

        Scripts.ListMonsterLocations(b); return;
        Scripts.MonsterTypeList(b); return;


        b.CreateMonsterPages(); return;

        Scripts.DumpGeometryTriggers(b); return;
        Scripts.DumpDat(b, "GeometryAttack.dat64"); return;

        //Scripts.SkillContextFlags(b); return;
        Scripts.GrantedEffectMonsterSkillShape(b); return;

        


        Scripts.ListDatRowCounts(b); return;
        Scripts.ListIdles(); return;

        //Scripts.ActiveSkillTypes(b); return;
        //Scripts.UniqueArt2(b); return;

        /*
        Scripts.ListMonsterColumns(b);
        Scripts.ListMonster(b, 4228); //enraptured crab medium
        Scripts.ListMonster(b, 4234); //enraptured crab large
        Scripts.ListMonster(b, 416); //rhoa mare
        Scripts.ListMonster(b, 424); //tercel rhoa
        Scripts.ListMonster(b, 59); // oaks devoted
        */
        //Scripts.MonsterBaseStats(b); return;







        //BLESSED SISTER CHAMBER
        //DamageDoneTest(3581, 3312, 3081, 2808, 2560, 2355, 2092, 1819, 1580, 1331);
        //DamageDoneTest(3581, 3353, 3079, 2883, 2646, 2438, 2172, 1921, 1656, 1390, 1186, 976, 755, 519, 246);
        //DamageDoneTest(3581, 3347, 3140, 2940, 2686, 2498, 2285, 2058, 1798, 1532, 1266, 1068, 834, 571, 340);

        //GALVANIC RIBBON SOLARIS 1
        //DamageDoneTest(3814, 3781, 3766, 3740, 3714, 3695, 3675, 3651, 3624, 3589, 3574, 3549, 3533, 3512, 3490, 3475, 3445, 3420, 3394, 3379, 3350, 3336, 3310, 3282, 3249);

        //RHOA MARE
        //DamageDoneTest(3911, 3863, 3834, 3797, 3768, 3739, 3702, 3674, 3637, 3605, 3569, 3542, 3505, 3476, 3444, 3415, 3375, 3343, 3306, 3279, 3251, 3223, 3190, 3153, 3120, 3091, 3056, 3023, 2997, 2962, 2937, 2908, 2876, 2849, 2812, 2781, 2747, 2717, 2687, 2652, 2619, 2587, 2553, 2519, 2485, 2456, 2427, 2396, 2367, 2338, 2313, 2284, 2254, 2223, 2186, 2160, 2130, 2105, 2073, 2048, 2019, 1988, 1960, 1923, 1892, 1865, 1830, 1805, 1773, 1739, 1713, 1680, 1645, 1617, 1590, 1556, 1530, 1493, 1457, 1432, 1395, 1368, 1331, 1301, 1270, 1229, 1196, 1167, 1140, 1113, 1079, 1054, 1029, 992, 958, 924, 894, 863, 822, 781, 746, 716, 691, 658, 623, 589, 555);

        /*
        DamageDoneTest(59, 51, 40, 29, 20, 9);
        DamageDoneTest(65, 55, 48, 40, 29, 18, 9);
        DamageDoneTest(66, 56, 45, 37, 26, 15, 8);
        DamageDoneTest(65, 56, 47, 39, 31, 21, 10);
        DamageDoneTest(66, 57, 48, 41, 34, 25, 18, 9);
        DamageDoneTest(65, 58, 45, 36, 27, 18, 10, 1);
        */


        //DEPRAVED ENFORCER L66
        /*
        DamageDoneTest(3911, 3134, 2539, 1946, 1372, 712, 76);
        DamageDoneTest(3891, 3311, 2616, 1975, 1402, 751);
        DamageDoneTest(3911, 3415, 2919, 2343, 1863, 1325, 642);
        DamageDoneTest(3842, 3436, 2901, 2409, 1715, 1018, 330);
        DamageDoneTest(3823, 3427, 2909, 2199, 1592, 1040, 407);
        DamageDoneTest(3900, 3393, 2795, 2218, 1636, 1132, 578);
        DamageDoneTest(3905, 3264, 2668, 2166, 1539, 944, 463);
        DamageDoneTest(3790, 3243, 2654, 2057, 1440, 779);
        DamageDoneTest(3773, 3351, 2876, 2290, 1677, 1187, 400);
        DamageDoneTest(3893, 3236, 2676, 2087, 1380, 733);
        DamageDoneTest(3894, 3229, 2562, 1693, 1120, 497);
        DamageDoneTest(3823, 3267, 2573, 1965, 1363, 860);
        DamageDoneTest(3855, 3334, 2825, 2208, 1510, 832);
        DamageDoneTest(3826, 3009, 2509, 1879, 1401, 777);
        DamageDoneTest(3346, 2658, 1962, 1381, 689);
        DamageDoneTest(3755, 3426, 2826, 2313, 1819, 1183, 627);
        DamageDoneTest(3787, 3306, 2816, 2246, 1587, 984, 284);
        */

        //ENRAPTURED CRAB L19
        //DamageDoneTest(3895, 3874, 3852, 3832, 3814, 3794, 3772, 3751, 3732, 3709, 3693, 3674, 3654, 3638, 3621, 3601, 3583, 3565, 3547, 3525, 3504, 3488, 3471, 3451, 3433, 3417, 3395, 3379, 3357, 3335, 3318, 3298, 3281, 3261, 3245, 3229, 3213, 3196, 3175, 3155, 3137, 3117, 3101, 3084, 3065, 3048, 3032, 3013, 2992, 2973, 2952, 2936, 2920, 2901, 2880, 2858, 2842, 2824, 2804, 2785, 2767, 2745, 2728, 2712, 2690, 2672, 2656, 2634, 2616, 2597, 2581, 2562, 2541, 2521, 2504, 2486, 2464, 2447, 2431, 2413);
        //RHOA MARE L19
        //DamageDoneTest(3911, 3874, 3848, 3821, 3785, 3748, 3715, 3678, 3642, 3602, 3566, 3534, 3507, 3477, 3448, 3413, 3381, 3352, 3317, 3292, 3261, 3224, 3193, 3160, 3126, 3100, 3063, 3034, 3000, 2965, 2932, 2895);
        //OAKS DEVOTED L19
        //DamageDoneTest(3911, 3876, 3849, 3814, 3789, 3756, 3730, 3705, 3671, 3635, 3611, 3587, 3557, 3524, 3498, 3472, 3439, 3410, 3383, 3350, 3319, 3283, 3257, 3224, 3188, 3162, 3133, 3098, 3075, 3052, 3026, 3002, 2975, 2952, 2923, 2892, 2864, 2828, 2798, 2762, 2728, 2701, 2668, 2633, 2601, 2569, 2539, 2504, 2479, 2446, 2417, 2391, 2358, 2324, 2298, 2264, 2239, 2208, 2182, 2148, 2118, 2093, 2060, 2024);
        //TERCEL RHOA L19
        //DamageDoneTest(3822, 3779, 3727, 3676, 3633, 3586, 3542, 3498, 3458, 3412, 3357, 3306, 3248, 3199, 3143, 3087, 3035, 2983, 2930, 2873, 2832, 2791, 2738, 2680, 2629, 2593, 2553, 2509, 2466, 2426, 2377, 2331, 2289, 2246, 2204, 2158, 2103, 2053, 2003, 1960, 1906, 1849, 1807, 1767, 1716, 1663, 1620, 1575, 1533, 1481, 1432, 1384, 1326, 1270, 1225, 1182, 1127, 1077, 1024, 981, 940, 889, 831, 783, 743, 687, 635, 592, 540, 482, 419, 376, 321, 271, 220, 172, 117, 61);
        //ENRAPTURED CRAB BIG L19
        //DamageDoneTest(3879, 3852, 3820, 3791, 3762, 3735, 3706, 3673, 3646, 3615, 3577, 3538, 3496, 3461, 3432, 3393, 3365, 3333, 3302, 3273, 3238, 3201, 3168, 3134, 3096, 3066, 3033, 2999, 2970, 2942, 2903, 2876, 2845, 2812, 2776, 2738, 2699, 2665, 2634, 2598, 2568, 2519, 2481, 2452, 2415, 2376, 2342, 2304, 2277, 2244, 2217, 2190, 2160, 2121, 2085, 2050, 2020, 1990, 1956, 1922, 1884, 1846, 1807, 1776, 1739, 1702, 1669, 1639, 1602, 1568, 1533, 1498, 1466, 1437, 1398, 1371, 1339, 1309, 1282, 1254, 1217, 1180, 1152, 1122, 1090, 1061, 1023, 990, 956, 921, 883, 852, 821, 786, 758, 724, 694, 656, 611, 577, 541, 509, 474, 438, 403, 367, 331, 296, 263, 236, 200, 172, 136, 101, 68, 30);

        //RHOA MARE
        DamageDoneTest(3847, 3763, 3553, 3337, 3165, 3012, 2821, 2652, 2503, 2346, 2170, 1995, 1839, 1683, 1515, 1342, 1184, 990, 803, 629, 414, 230);
        DamageDoneTest(3843, 3757, 3609, 3401, 3192, 2998, 2784, 2569, 2400, 2228, 2070, 1882, 1683, 1450, 1271, 1113, 897, 714, 498);
        DamageDoneTest(3748, 3595, 3399, 3204, 3034, 2873, 2654, 2450, 2291, 2126, 1967, 1773, 1616, 1399, 1226, 1027, 852, 702, 547, 340, 189);
        DamageDoneTest(3825, 3648, 3465, 3281, 3117, 2921, 2759, 2610, 2453, 2283, 2103, 1919, 1649, 1486, 1282, 1128, 975, 815, 637, 488, 283, 122);
        DamageDoneTest(3782, 3667, 3463, 3300, 3130, 2945, 2754, 2548, 2365, 2217, 2013, 1795, 1592, 1380, 1215, 1058, 862, 669, 518, 363);
        DamageDoneTest(3904, 3760, 3554, 3382, 3216, 3046, 2896, 2728, 2544, 2360, 2192, 2041, 1869, 1653, 1462, 1277, 1100, 896, 703, 526, 360, 170);
        return;


        HashSet<int> vals = new HashSet<int>();
        for(int dam = 100; dam < 500; dam++) {
            float d = dam / 10f;
            int low =(int)(d * 0.8 + 0.5f);
            int high = (int)(d * 1.2 + 0.5f);
            if (vals.Contains(low + high)) continue;
            Console.WriteLine($"{low} to {high} - {d}");
            vals.Add(low + high);
            
        }


        return;
        //SKILLDAMAGEMULT

        for (int i = 1; i <= 100; i++) {
            float dam = (3885209 + 360246 * (i - 1)) / 1000000f;

            Console.WriteLine($"{i} - {dam}");
        }

        for (int i = 1; i <= 100; i++) {
            double levelMult = (3885209 + 360246 * (i - 1)) / 1000000.0;
            Console.WriteLine($"Level {i} - {levelMult}");
            continue;
            Console.Write($"{i} - ");
            PrintDamage(i, 0.85, 0.03, 0.6, 1.8);
        }

    }

    static void DamageDoneTest(params int[] life) {
        for(int i = 0; i < life.Length - 1; i++) {
            Console.WriteLine(life[i] - life[i+1]);
        }
        Console.WriteLine();
    }

    static void PrintDamage(int level, double baseEffectiveness, double incrementalEffectiveness, double min = 0.8, double max = 1.2) {
        double levelMult = (3885209 + 360246 * (level - 1)) / 1000000.0;
        double damage = levelMult * baseEffectiveness * (float)Math.Pow(1 + incrementalEffectiveness, level - 1);
        int minDam = (int)(damage * min);
        int maxDam = (int)(damage * max);
        Console.WriteLine($"Deals {minDam} to {maxDam} damage");
    }
}