using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;

namespace Almanac.Almanac;

public static class IgnoreList
{
    public static List<string> serverIgnoreList = new();
    
    private static readonly string folderName = "Almanac";
    private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);

    public static void InitIgnoreList()
    {
        if (!Directory.Exists(Path.Combine(folderPath)))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "IgnoreList.yml");
        
        List<string> defaultList = new()
        {
            "#List out items prefabs to ignore:",
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
            "VegvisirShard_Bonemass"
        };

        if (!File.Exists(filePath))
        {
            File.WriteAllLines(filePath, defaultList);
        }

        List<string> ignoreList = new();
        foreach (string line in File.ReadLines(filePath))
        {
            if (line.StartsWith("#")) continue;
            ignoreList.Add(line);
        }
        
        serverIgnoreList = ignoreList;
    }
}