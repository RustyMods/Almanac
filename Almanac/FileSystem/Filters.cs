using System.Collections.Generic;
using System.IO;
using Almanac.Data;

namespace Almanac.FileSystem;

public static class Filters
{
    public static bool Ignore(string name) =>
        AlmanacPlugin._UseIgnoreList.Value is AlmanacPlugin.Toggle.On && m_filter.Contains(name);
    
    public static List<string> m_filter = new();
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
        "goblin_bed"
    };
    public static void InitFilters()
    {
        if (!File.Exists(AlmanacPaths.IgnorePath))
        {
            File.WriteAllLines(AlmanacPaths.IgnorePath, m_default);
        }
        m_filter.Clear();
        foreach (string line in File.ReadLines(AlmanacPaths.IgnorePath))
        {
            if (line.StartsWith("#")) continue;
            m_filter.Add(line);
        }
    }
}