using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Data;
using Almanac.FileSystem;

namespace Almanac.Achievements;

public static class CreatureLists
{
    private static readonly List<string> meadows = new() 
        { "Neck", "Greyling", "Boar", "Deer", "Eikthyr" };

    private static readonly List<string> blackForest = new() 
        { "Skeleton", "Greydwarf", "Greydwarf_Shaman", "Greydwarf_Elite", "Ghost", "Troll", "gdking" };

    private static readonly List<string> swamps = new()
        { "Skeleton_Poison", "Draugr", "Draugr_Ranged", "Draugr_Elite", "Blob", "BlobElite", "Wraith", "Abomination", "Surtling", "Bonemass" };

    private static readonly List<string> mountains = new()
        { "Wolf", "Ulv", "Fenring", "Fenring_Cultist", "StoneGolem", "Hatchling", "Bat", "Dragon" };

    private static readonly List<string> plains = new()
        { "Goblin", "GoblinShaman", "GoblinBrute", "Lox", "Deathsquito", "BlobTar", "GoblinKing" };

    private static readonly List<string> mistlands = new()
        { "Seeker", "SeekerBrood", "SeekerBrute", "Tick", "Gjall", "Dverger", "DvergerMageFire", "DvergerMageIce", "DvergerMageSupport", "Hare", "Hen", "SeekerQueen" };

    private static readonly List<string> ocean = new()
        { "Serpent" };

    public static Dictionary<Heightmap.Biome, List<CreatureDataCollector.CreatureData>> BiomeCreatureMap = new();

    public static List<CreatureDataCollector.CreatureData> GetBiomeCreatures(Heightmap.Biome land) => BiomeCreatureMap[land];

    public static void InitCreatureLists()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Initializing creature list");
        foreach (Heightmap.Biome land in Enum.GetValues(typeof(Heightmap.Biome)))
        {
            if (land is Heightmap.Biome.None) continue;
            string fileName = land + ".yml";
            string filePath = AlmanacPaths.CreatureFolderPath + Path.DirectorySeparatorChar + fileName;
            if (!File.Exists(filePath))
            {
                switch (land)
                {
                    case Heightmap.Biome.Meadows:
                        File.WriteAllLines(filePath, meadows);
                        break;
                    case Heightmap.Biome.BlackForest:
                        File.WriteAllLines(filePath, blackForest);
                        break;
                    case Heightmap.Biome.Swamp:
                        File.WriteAllLines(filePath, swamps);
                        break;
                    case Heightmap.Biome.Mountain:
                        File.WriteAllLines(filePath, mountains);
                        break;
                    case Heightmap.Biome.Plains:
                        File.WriteAllLines(filePath, plains);
                        break;
                    case Heightmap.Biome.Mistlands:
                        File.WriteAllLines(filePath, mistlands);
                        break;
                    case Heightmap.Biome.Ocean:
                        File.WriteAllLines(filePath, ocean);
                        break;
                    default:
                        File.WriteAllLines(filePath, new List<string>(){"#Add creature prefabs to be used for achievements:"});
                        break;
                }
            }

            try
            {
                List<string> data = File.ReadAllLines(filePath).ToList();
                BiomeCreatureMap[land] = ValidatedPrefabs(data);
            }
            catch (Exception)
            {
                AlmanacPlugin.AlmanacLogger.LogDebug("Failed to read creature list file: " + filePath);
            }
        }
    }

    public static List<CreatureDataCollector.CreatureData> ValidatedPrefabs(List<string> input)
    {
        List<CreatureDataCollector.CreatureData> output = new();
        foreach (string name in input)
        {
            if (name.StartsWith("#")) continue;
            CreatureDataCollector.CreatureData? creature = CreatureDataCollector.tempCreatureData.Find(creature => creature.name.ToLower() == name.ToLower());

            if (creature == null) continue;
            output.Add(creature);
        }

        return output;
    }
}