using System.Collections.Generic;
using System.IO;
using Almanac.Data;

namespace Almanac.FileSystem;

public static class Filters
{
    public static List<string> FilterList = new();
    private static readonly List<string> defaultList = new()
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
        "ShieldIronSquare"
    };
    public static void InitFilters()
    {
        ItemDataCollector.ClearCachedItemDrops();
        if (!File.Exists(AlmanacPaths.IgnorePath))
        {
            File.WriteAllLines(AlmanacPaths.IgnorePath, defaultList);
        }
        FilterList.Clear();
        foreach (string line in File.ReadLines(AlmanacPaths.IgnorePath))
        {
            if (line.StartsWith("#")) continue;
            FilterList.Add(line);
        }
    }
}