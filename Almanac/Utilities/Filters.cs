using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Managers;

namespace Almanac.Utilities;

public static class Filters
{
    public static bool Ignore(string name)
    {
        if (!Configs.UseIgnoreList) return false;
        return filters.Contains(name) || specialFilters.Any(name.EndsWith);
    }

    private static readonly List<string> filters = new();
    private static readonly List<string> m_default = new()
    {
        "#List out prefabs to ignore:",
        "StaminaUpgrade_Greydwarf",
        "StaminaUpgrade_Troll",
        "StaminaUpgrade_Wraith",
        "IronOre",
        "DvergerArbalest_shoot",
        "DvergerArbalest",
        "CapeTest",
        "SledgeCheat",
        "SwordCheat",
        "HealthUpgrade_Bonemass",
        "HealthUpgrade_GDKing",
        "guard_stone_test",
        "Trailership",
        "Player",
        "TorchMist",
        "NPC_HelmetIron_Worn0",
        "NPC_HelmetBronze_Worn0",
        "NPC_ArmorIronChest_Worn",
        "NPC_ArmorIronLegs_Worn",
        "TrainingDummy",
        "VegvisirShard_Bonemass",
        "CapeOdin",
        "HelmetOdin",
        "TankardOdin",
        "ShieldIronSquare",
        "SwordIronFire",
        "goblin_bed",
        "DvergerTest",
        "Hive",
        "Pot_Shard_Red",
        "Goblin_Gem",
        "Leech_cave",
        "staff_greenroots_tentaroot",
        "Morgen_NonSleeping",
        "Skeleton_NoArcher",
        "TheHive",
        "portal",
        "raise",
        "mud_road",
        "cultivate",
        "path",
        "paved_road",
        "fire_pit_haldor",
        "fire_pit_hildir",
        "ShieldKnight",
        "*_nochest"
    };

    private static readonly List<string> specialFilters = new();
    public static void Setup()
    {
        if (!File.Exists(AlmanacPaths.IgnorePath))
        {
            File.WriteAllLines(AlmanacPaths.IgnorePath, m_default);
        }
        filters.Clear();
        foreach (string line in File.ReadLines(AlmanacPaths.IgnorePath))
        {
            if (line.StartsWith("#")) continue;
            if (line.StartsWith("*"))
            {
                specialFilters.Add(line.Replace("*", string.Empty));
            }
            else filters.Add(line);
        }
    }
}